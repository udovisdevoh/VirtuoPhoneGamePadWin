using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Vorbis;

namespace VirtuoPhone;

/// <summary>
/// Real low-latency <see cref="ISoundPool"/> built on NAudio + WASAPI (shared, event-driven).
///
/// Design (SoundPool-style): each <see cref="Load"/> fully decodes an <c>.ogg</c> into an in-memory
/// mono float buffer kept at its <b>native sample rate</b>. Each <see cref="Play"/> spawns a lightweight
/// <see cref="Voice"/> that reads that shared buffer with its own position, pitch and L/R gains. A single
/// <see cref="Mixer"/> is pulled by the WASAPI render thread and sums all active voices.
///
/// Quality choices:
/// * <b>One interpolation stage per voice.</b> Buffers stay at native rate; the source→output rate ratio
///   is folded into the playback step, so a note is resampled exactly once (never twice).
/// * <b>Cubic (Catmull-Rom) interpolation</b> at playback — much cleaner than linear for pitch-shifted notes.
/// * <b>Headroom + transparent soft-knee.</b> A per-poly headroom gain keeps normal playing in the linear
///   region; a soft knee only bends peaks above ~0.8, so the output never clips yet stays uncolored at
///   normal levels.
/// </summary>
public sealed class NAudioSoundPool : ISoundPool, IDisposable
{
    private sealed class SoundBuffer
    {
        public float[] Mono = Array.Empty<float>();   // decoded + downmixed to mono, native rate
        public int SampleRate = 48000;
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

        public double Step => Pitch * RateRatio;
    }

    private readonly ConcurrentDictionary<int, SoundBuffer> buffers = new();
    private readonly ConcurrentDictionary<int, Voice> voices = new();
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

        // Headroom: keep ~6 uncorrelated voices under 0 dBFS (sqrt sum), so normal chords stay linear.
        MasterVolume = 1f / MathF.Sqrt(Math.Min(maxVoices, 6));

        // The whole sample bank is 44.1 kHz. Run the engine at 44.1 kHz so an unshifted note plays at a
        // step of exactly 1.0 — bit-exact, zero rate conversion (only genuine pitch shifts interpolate).
        // Prefer WASAPI exclusive so the OS doesn't resample either; fall back to shared if unavailable.
        outputSampleRate = 44100;
        var mixer = new Mixer(this, WaveFormat.CreateIeeeFloatWaveFormat(outputSampleRate, OutputChannels));

        // Lowest output latency first: prefer exclusive (bit-exact, no OS resample/effects) with the
        // smallest buffer the device accepts, stepping up only as needed; shared is the last resort.
        (AudioClientShareMode share, int latencyMs, string label)[] candidates =
        {
            (AudioClientShareMode.Exclusive, 5,  "exclusive 5 ms (bit-exact)"),
            (AudioClientShareMode.Exclusive, 10, "exclusive 10 ms (bit-exact)"),
            (AudioClientShareMode.Exclusive, 20, "exclusive 20 ms (bit-exact)"),
            (AudioClientShareMode.Shared,    10, "shared 10 ms (OS may resample)"),
            (AudioClientShareMode.Shared,    30, "shared 30 ms (OS may resample)"),
        };

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

        int frames = channels > 0 ? interleaved.Count / channels : 0;
        float[] mono = new float[frames];
        for (int f = 0; f < frames; f++)
        {
            float s = 0f;
            for (int c = 0; c < channels; c++) s += interleaved[f * channels + c];
            mono[f] = s / channels;
        }

        return new SoundBuffer { Mono = mono, SampleRate = sourceRate };
    }

    public int Play(int soundId, float leftVolume, float rightVolume, int priority, int loop, float rate)
    {
        if (!buffers.TryGetValue(soundId, out var buf) || buf.Mono.Length == 0) return 0;

        // Cap simultaneous voices (voice-stealing: drop the oldest) so polyphony can't run away.
        while (voices.Count >= maxVoices)
        {
            int oldest = int.MaxValue;
            foreach (int k in voices.Keys) if (k < oldest) oldest = k;
            if (oldest == int.MaxValue || !voices.TryRemove(oldest, out _)) break;
        }

        int streamId = System.Threading.Interlocked.Increment(ref nextStreamId);
        voices[streamId] = new Voice
        {
            Src = buf.Mono,
            Position = 0,
            Pitch = rate <= 0f ? 1.0 : rate,
            RateRatio = (double)buf.SampleRate / outputSampleRate,
            VolL = leftVolume,
            VolR = rightVolume,
            Loop = loop != 0,
        };
        return streamId;
    }

    public void Stop(int streamId) => voices.TryRemove(streamId, out _);

    public void SetVolume(int streamId, float left, float right)
    {
        if (voices.TryGetValue(streamId, out var v)) { v.VolL = left; v.VolR = right; }
    }

    public void SetRate(int streamId, float rate)
    {
        if (voices.TryGetValue(streamId, out var v)) v.Pitch = rate <= 0f ? 1.0 : rate;
    }

    public void Release() => Dispose();

    public void Dispose()
    {
        try { output.Stop(); } catch { /* ignore */ }
        output.Dispose();
        voices.Clear();
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

    /// <summary>Transparent below ~0.8, softly compresses peaks up to (but never past) 1.0.</summary>
    private static float SoftKnee(float x)
    {
        const float T = 0.8f;
        float a = MathF.Abs(x);
        if (a <= T) return x;
        float over = (a - T) / (1f - T);
        return MathF.Sign(x) * (T + (1f - T) * MathF.Tanh(over));
    }

    /// <summary>Master mixer pulled by the WASAPI render thread; sums every active voice.</summary>
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

            foreach (var kv in pool.voices)
            {
                Voice v = kv.Value;
                float[] src = v.Src;
                int srcLen = src.Length;
                if (srcLen == 0) { pool.voices.TryRemove(kv.Key, out _); continue; }

                double pos = v.Position;
                double step = v.Step;
                float volL = v.VolL, volR = v.VolR;
                int o = offset;
                bool finished = false;

                for (int f = 0; f < frames; f++)
                {
                    if (pos >= srcLen)
                    {
                        if (v.Loop) { pos %= srcLen; }
                        else { finished = true; break; }
                    }
                    float s = SampleCubic(src, srcLen, pos);
                    buffer[o++] += s * volL;
                    buffer[o++] += s * volR;
                    pos += step;
                }

                v.Position = pos;
                if (finished) pool.voices.TryRemove(kv.Key, out _);
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
