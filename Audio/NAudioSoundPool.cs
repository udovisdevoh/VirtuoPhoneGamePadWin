using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Vorbis;

namespace VirtuoPhone;

/// <summary>
/// Real low-latency <see cref="ISoundPool"/> built on NAudio + WASAPI (exclusive-preferred, event-driven).
///
/// Design (SoundPool-style): each <see cref="Load"/> fully decodes an <c>.ogg</c> into an in-memory mono
/// float buffer kept at its <b>native 44.1 kHz</b>. Each <see cref="Play"/> claims a slot in a fixed,
/// pre-allocated <see cref="Voice"/> pool; a single <see cref="Mixer"/> is pulled by the WASAPI render
/// thread and sums the active voices.
///
/// Real-time correctness (this is why low latency is glitch-free):
/// * <b>Zero allocation on the audio thread.</b> Voices live in a fixed array iterated with a plain loop —
///   no per-buffer enumerator/garbage — so the .NET GC never pauses us into an underrun ("pop").
/// * <b>One interpolation stage.</b> Engine runs at 44.1 kHz (the sample rate): an unshifted note steps by
///   exactly 1.0 → sample-accurate, no resampling; only genuine pitch shifts interpolate (cubic).
/// * <b>Headroom + soft-knee</b> on the mix bus so stacked voices never hard-clip.
///
/// Concurrency: control calls (Play/Stop/SetRate/SetVolume) and the mixer share a short <see cref="gate"/>
/// lock. Critical sections are tiny and allocation-free, so the render thread is never blocked meaningfully.
/// </summary>
public sealed class NAudioSoundPool : ISoundPool, IDisposable
{
    private sealed class SoundBuffer
    {
        public float[] Mono = Array.Empty<float>();   // decoded, native rate (44.1 kHz)
        public int SampleRate = 44100;
    }

    private sealed class Voice
    {
        public float[] Src = Array.Empty<float>();
        public double Position;        // in source samples
        public double Pitch = 1.0;     // semitone rate multiplier from the caller
        public double RateRatio = 1.0; // sourceRate / outputRate (folded resampling)
        public float VolL = 1f;
        public float VolR = 1f;
        public bool Loop;
        public int StreamId;           // 0 = free; identifies the voice for Stop/SetRate/SetVolume
        public bool Active;

        public double Step => Pitch * RateRatio;
    }

    private readonly ConcurrentDictionary<int, SoundBuffer> buffers = new();
    private readonly Voice[] slots;                 // fixed, pre-allocated voice pool
    private readonly object gate = new();
    private int nextSoundId;
    private int nextStreamId;

    private readonly int outputSampleRate;
    private readonly int maxVoices;
    private const int OutputChannels = 2;

    /// <summary>Mix-bus headroom gain applied before the soft-knee. Set from polyphony so a few
    /// simultaneous notes stay in the clean linear region; raise/lower to taste. Output can't clip.</summary>
    public float MasterVolume { get; set; }

    private readonly string sampleDir;
    private readonly IWavePlayer output;

    public NAudioSoundPool(int polyphony)
    {
        sampleDir = Path.Combine(AppContext.BaseDirectory, "res", "raw");
        maxVoices = Math.Max(1, polyphony);
        MasterVolume = 1f / MathF.Sqrt(Math.Min(maxVoices, 6));   // headroom for ~6 uncorrelated voices

        slots = new Voice[maxVoices];
        for (int i = 0; i < slots.Length; i++) slots[i] = new Voice();

        // Output mode decides the engine sample rate. Default = SHARED so the Windows volume slider works,
        // note attacks aren't affected by exclusive stream start/stop, and we coexist with other apps. Set
        // VP_AUDIO_MODE=exclusive for a bit-exact, lowest-latency path that BYPASSES the Windows volume mixer
        // and takes the device exclusively. VP_LATENCY_MS forces a buffer size.
        bool exclusive = string.Equals(Environment.GetEnvironmentVariable("VP_AUDIO_MODE"), "exclusive",
                                       StringComparison.OrdinalIgnoreCase);
        bool forced = int.TryParse(Environment.GetEnvironmentVariable("VP_LATENCY_MS"), out int forcedMs) && forcedMs > 0;

        // Engine rate: exclusive → native 44.1 kHz (bit-exact). Shared → the device's mix rate, so the OS does
        // NO per-buffer resampling; instead we resample each 44.1 kHz sample to this rate ONCE, at load (see
        // LoadBuffer). Either way an unshifted note plays at step 1.0.
        outputSampleRate = exclusive ? 44100 : QueryDeviceMixRate(48000);
        var mixer = new Mixer(this, WaveFormat.CreateIeeeFloatWaveFormat(outputSampleRate, OutputChannels));

        var candidates = new List<(AudioClientShareMode share, int latencyMs, string label)>();
        if (exclusive)
        {
            foreach (int ms in forced ? new[] { forcedMs } : new[] { 10, 15, 20 })
                candidates.Add((AudioClientShareMode.Exclusive, ms, $"exclusive {ms} ms (bit-exact, no Windows volume)"));
            candidates.Add((AudioClientShareMode.Shared, forced ? forcedMs : 20, "shared (fallback)"));
        }
        else
        {
            foreach (int ms in forced ? new[] { forcedMs } : new[] { 10, 20, 30 })
                candidates.Add((AudioClientShareMode.Shared, ms, $"shared {ms} ms (Windows volume works)"));
            candidates.Add((AudioClientShareMode.Exclusive, forced ? forcedMs : 10, "exclusive (fallback)"));
        }

        IWavePlayer? chosen = null;
        string mode = "none";
        foreach (var c in candidates)
        {
            WasapiOut? wo = null;
            try
            {
                wo = new WasapiOut(c.share, useEventSync: true, c.latencyMs);
                wo.Init(mixer);
                wo.Play();
                chosen = wo;
                mode = c.label;
                break;
            }
            catch { wo?.Dispose(); }
        }
        output = chosen ?? throw new InvalidOperationException("Could not open any WASAPI output device.");

        Console.WriteLine($"[audio] NAudio/WASAPI {mode}, {outputSampleRate} Hz / {OutputChannels} ch, " +
                          $"polyphony {maxVoices}, master {MasterVolume:0.00}");
    }

    public int Load(int resourceId, int priority)
    {
        int id = System.Threading.Interlocked.Increment(ref nextSoundId);
        buffers[id] = LoadBuffer(resourceId);
        return id;
    }

    private SoundBuffer LoadBuffer(int resourceId)
    {
        string? file = Models.R.GetFileName(resourceId);
        if (file is null) return new SoundBuffer();

        string path = Path.Combine(sampleDir, file);
        if (!File.Exists(path))
        {
            Console.WriteLine($"[audio] missing sample: {path}");
            return new SoundBuffer();
        }

        using var reader = new VorbisWaveReader(path);   // ISampleProvider, IEEE float
        int channels = reader.WaveFormat.Channels;
        int sourceRate = reader.WaveFormat.SampleRate;

        var interleaved = new List<float>(1 << 16);
        float[] tmp = new float[sourceRate * Math.Max(1, channels)];   // ~1 s chunks
        int read;
        while ((read = reader.Read(tmp, 0, tmp.Length)) > 0)
            for (int i = 0; i < read; i++) interleaved.Add(tmp[i]);

        // Samples are mono; if a stereo file ever appears, average to mono (no filtering of mono content).
        float[] mono;
        if (channels == 1)
        {
            mono = interleaved.ToArray();
        }
        else
        {
            int frames = channels > 0 ? interleaved.Count / channels : 0;
            mono = new float[frames];
            for (int f = 0; f < frames; f++)
            {
                float s = 0f;
                for (int c = 0; c < channels; c++) s += interleaved[f * channels + c];
                mono[f] = s / channels;
            }
        }

        // Resample ONCE here, in RAM, to the engine/output rate — so playback never resamples (only genuine
        // pitch shifts interpolate) and, in shared mode, the OS doesn't resample the mix on every buffer.
        return new SoundBuffer { Mono = ResampleTo(mono, sourceRate, outputSampleRate), SampleRate = outputSampleRate };
    }

    public int Play(int soundId, float leftVolume, float rightVolume, int priority, int loop, float rate)
    {
        if (!buffers.TryGetValue(soundId, out var buf) || buf.Mono.Length == 0) return 0;

        lock (gate)
        {
            // Prefer a free slot; otherwise steal the oldest active voice (smallest StreamId).
            Voice slot = slots[0];
            bool foundFree = false;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].Active) { slot = slots[i]; foundFree = true; break; }
            }
            if (!foundFree)
            {
                for (int i = 1; i < slots.Length; i++)
                    if (slots[i].StreamId < slot.StreamId) slot = slots[i];
            }

            int streamId = ++nextStreamId;
            if (streamId <= 0) streamId = nextStreamId = 1;

            slot.Active = false;    // pause reads while we repopulate (we hold the gate anyway)
            slot.Src = buf.Mono;
            slot.Position = 0;
            slot.Pitch = rate <= 0f ? 1.0 : rate;
            slot.RateRatio = (double)buf.SampleRate / outputSampleRate;
            slot.VolL = leftVolume;
            slot.VolR = rightVolume;
            slot.Loop = loop != 0;
            slot.StreamId = streamId;
            slot.Active = true;
            return streamId;
        }
    }

    public void Stop(int streamId)
    {
        lock (gate)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].Active && slots[i].StreamId == streamId) { slots[i].Active = false; return; }
        }
    }

    public void SetVolume(int streamId, float left, float right)
    {
        lock (gate)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].Active && slots[i].StreamId == streamId) { slots[i].VolL = left; slots[i].VolR = right; return; }
        }
    }

    public void SetRate(int streamId, float rate)
    {
        lock (gate)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].Active && slots[i].StreamId == streamId) { slots[i].Pitch = rate <= 0f ? 1.0 : rate; return; }
        }
    }

    public void Release() => Dispose();

    public void Dispose()
    {
        try { output.Stop(); } catch { /* ignore */ }
        output.Dispose();
        lock (gate) { foreach (var v in slots) v.Active = false; }
        buffers.Clear();
    }

    /// <summary>Catmull-Rom cubic interpolation of <paramref name="src"/> at fractional index pos.</summary>
    private static float SampleCubic(float[] src, int len, double pos)
    {
        int i1 = (int)pos;
        float t = (float)(pos - i1);
        int i0 = i1 > 0 ? i1 - 1 : 0;
        int i2 = i1 + 1 < len ? i1 + 1 : len - 1;
        int i3 = i1 + 2 < len ? i1 + 2 : len - 1;
        float p0 = src[i0], p1 = src[i1], p2 = src[i2], p3 = src[i3];

        float c1 = 0.5f * (p2 - p0);
        float c2 = p0 - 2.5f * p1 + 2f * p2 - 0.5f * p3;
        float c3 = 0.5f * (p3 - p0) + 1.5f * (p1 - p2);
        return ((c3 * t + c2) * t + c1) * t + p1;
    }

    /// <summary>Cubic-resample a mono buffer from srcRate to dstRate. Done once, at load — never at playback.</summary>
    private static float[] ResampleTo(float[] src, int srcRate, int dstRate)
    {
        if (srcRate == dstRate || src.Length == 0) return src;
        int dstLen = (int)Math.Round(src.Length * (double)dstRate / srcRate);
        var dst = new float[dstLen];
        double step = (double)srcRate / dstRate;    // source samples advanced per output sample
        double pos = 0;
        for (int i = 0; i < dstLen; i++) { dst[i] = SampleCubic(src, src.Length, pos); pos += step; }
        return dst;
    }

    /// <summary>The shared-mode mix sample rate of the default render device (so we can match it, no OS resample).</summary>
    private static int QueryDeviceMixRate(int fallback)
    {
        try
        {
            using var en = new MMDeviceEnumerator();
            using var dev = en.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return dev.AudioClient.MixFormat.SampleRate;
        }
        catch { return fallback; }
    }

    /// <summary>Transparent below ~0.8, softly compresses peaks up to (but never past) 1.0.</summary>
    private static float SoftKnee(float x)
    {
        const float T = 0.8f;
        float a = MathF.Abs(x);
        if (a <= T) return x;
        float over = (a - T) / (1f - T);
        return MathF.Sign(x) * (T + (1f - T) * MathF.Tanh(over));
    }

    /// <summary>Master mixer pulled by the WASAPI render thread; sums every active voice. No allocations.</summary>
    private sealed class Mixer : ISampleProvider
    {
        private readonly NAudioSoundPool pool;
        public WaveFormat WaveFormat { get; }

        public Mixer(NAudioSoundPool pool, WaveFormat format)
        {
            this.pool = pool;
            WaveFormat = format;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            Array.Clear(buffer, offset, count);
            int frames = count / OutputChannels;
            Voice[] slots = pool.slots;

            lock (pool.gate)
            {
                for (int vi = 0; vi < slots.Length; vi++)
                {
                    Voice v = slots[vi];
                    if (!v.Active) continue;
                    float[] src = v.Src;
                    int srcLen = src.Length;
                    if (srcLen == 0) { v.Active = false; continue; }

                    double pos = v.Position;
                    double step = v.Step;
                    float volL = v.VolL, volR = v.VolR;
                    int o = offset;
                    bool ended = false;

                    for (int f = 0; f < frames; f++)
                    {
                        if (pos >= srcLen)
                        {
                            if (v.Loop) { pos %= srcLen; }
                            else { ended = true; break; }   // natural end (sample already faded to ~0)
                        }
                        float s = SampleCubic(src, srcLen, pos);
                        buffer[o++] += s * volL;
                        buffer[o++] += s * volR;
                        pos += step;
                    }

                    v.Position = pos;
                    if (ended) v.Active = false;
                }
            }

            // Headroom gain, then a soft knee that only engages on peaks — clean in the normal range,
            // and the output can never exceed 0 dBFS.
            float master = pool.MasterVolume;
            int end = offset + count;
            for (int i = offset; i < end; i++)
                buffer[i] = SoftKnee(buffer[i] * master);

            return count;   // never end the stream: keep the synth open
        }
    }
}
