using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Vorbis;

namespace VirtuoPhone;

/// <summary>
/// Low-latency, polyphonic <see cref="ISoundPool"/> built on NAudio. <b>Mixing and output are done by
/// NAudio's own <see cref="MixingSampleProvider"/> + WASAPI</b> — we no longer hand-roll the mixer (a custom
/// mixer fed directly to WASAPI shared-mode caused crackle; NAudio's is proven clean).
///
/// Each <see cref="Load"/> pre-decodes an <c>.ogg</c> into a mono float buffer, resampled once (at load) to
/// the output/device rate. Each <see cref="Play"/> adds a <see cref="VoiceSampleProvider"/> (pitch, envelope,
/// glissando) as a mixer input; the mixer removes it when it ends.
/// </summary>
public sealed class NAudioSoundPool : ISoundPool, IDisposable
{
    private sealed class SoundBuffer
    {
        public float[] Mono = Array.Empty<float>();   // decoded, resampled to the output rate
        public int SampleRate = 44100;
    }

    private readonly ConcurrentDictionary<int, SoundBuffer> buffers = new();
    private readonly ConcurrentDictionary<int, VoiceSampleProvider> voices = new();
    private int nextSoundId;
    private int nextStreamId;

    private readonly int outputSampleRate;
    private readonly int maxVoices;
    private const int OutputChannels = 2;

    private readonly string sampleDir;
    private readonly MixingSampleProvider mixer;
    private readonly IWavePlayer output;
    private readonly object voicesLock = new();

    public NAudioSoundPool(int polyphony)
    {
        sampleDir = Path.Combine(AppContext.BaseDirectory, "res", "raw");
        maxVoices = Math.Max(1, polyphony);

        // Output mode: default SHARED (Windows volume works); VP_AUDIO_MODE=exclusive for bit-exact/no-OS-mix.
        bool exclusive = string.Equals(Environment.GetEnvironmentVariable("VP_AUDIO_MODE"), "exclusive",
                                       StringComparison.OrdinalIgnoreCase);
        bool forced = int.TryParse(Environment.GetEnvironmentVariable("VP_LATENCY_MS"), out int forcedMs) && forcedMs > 0;

        // Engine rate: exclusive → 44.1 kHz (bit-exact). Shared → the device mix rate (samples are resampled
        // to it once at load, so the OS does no per-buffer resampling).
        outputSampleRate = exclusive ? 44100 : QueryDeviceMixRate(48000);

        // NAudio owns the mixing and the fill-with-silence between notes (ReadFully). Master headroom keeps
        // a few simultaneous voices below 0 dBFS.
        mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(outputSampleRate, OutputChannels))
        {
            ReadFully = true,
        };
        mixer.MixerInputEnded += (sender, e) =>
        {
            if (e.SampleProvider is VoiceSampleProvider v) voices.TryRemove(v.StreamId, out _);
        };
        var master = new VolumeSampleProvider(mixer) { Volume = 1f / MathF.Sqrt(Math.Min(maxVoices, 6)) };

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
                wo.Init(master);
                wo.Play();
                chosen = wo;
                mode = c.label;
                break;
            }
            catch { wo?.Dispose(); }
        }
        output = chosen ?? throw new InvalidOperationException("Could not open any WASAPI output device.");

        Console.WriteLine($"[audio] NAudio MixingSampleProvider + WASAPI {mode}, {outputSampleRate} Hz / {OutputChannels} ch, " +
                          $"polyphony {maxVoices}, master {master.Volume:0.00}");
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
        float[] tmp = new float[sourceRate * Math.Max(1, channels)];
        int read;
        while ((read = reader.Read(tmp, 0, tmp.Length)) > 0)
            for (int i = 0; i < read; i++) interleaved.Add(tmp[i]);

        // Samples are mono; average to mono if a stereo file ever appears.
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

        // Resample once, in RAM, to the output rate (so playback never resamples and the OS doesn't either).
        return new SoundBuffer { Mono = ResampleTo(mono, sourceRate, outputSampleRate), SampleRate = outputSampleRate };
    }

    public int Play(int soundId, float leftVolume, float rightVolume, int priority, int loop, float rate,
                    float attackSeconds, float releaseSeconds)
    {
        if (!buffers.TryGetValue(soundId, out var buf) || buf.Mono.Length == 0) return 0;

        int streamId = System.Threading.Interlocked.Increment(ref nextStreamId);
        var voice = new VoiceSampleProvider(streamId, buf.Mono, buf.SampleRate, outputSampleRate,
                                            leftVolume, rightVolume, loop != 0, rate, attackSeconds, releaseSeconds);
        lock (voicesLock)
        {
            // Voice cap: drop the oldest active voice to make room.
            while (voices.Count >= maxVoices)
            {
                int oldest = int.MaxValue;
                foreach (int k in voices.Keys) if (k < oldest) oldest = k;
                if (oldest != int.MaxValue && voices.TryRemove(oldest, out var ov)) mixer.RemoveMixerInput(ov);
                else break;
            }
            voices[streamId] = voice;
            mixer.AddMixerInput(voice);
        }
        return streamId;
    }

    public void Stop(int streamId)
    {
        if (voices.TryGetValue(streamId, out var v)) v.Release();
    }

    public void SetVolume(int streamId, float left, float right)
    {
        if (voices.TryGetValue(streamId, out var v)) v.SetVolume(left, right);
    }

    public void SetRate(int streamId, float rate)
    {
        if (voices.TryGetValue(streamId, out var v)) v.SetRate(rate);
    }

    public void GlideRate(int streamId, float rateFactor, float glideSeconds)
    {
        if (voices.TryGetValue(streamId, out var v)) v.GlideRate(rateFactor, glideSeconds);
    }

    public void GlideToRate(int streamId, float targetRate, float glideSeconds)
    {
        if (voices.TryGetValue(streamId, out var v)) v.GlideToRate(targetRate, glideSeconds);
    }

    public void Release() => Dispose();

    public void Dispose()
    {
        try { output.Stop(); } catch { /* ignore */ }
        output.Dispose();
        voices.Clear();
        buffers.Clear();
    }

    /// <summary>Cubic-resample a mono buffer from srcRate to dstRate. Done once, at load — never at playback.</summary>
    private static float[] ResampleTo(float[] src, int srcRate, int dstRate)
    {
        if (srcRate == dstRate || src.Length == 0) return src;
        int dstLen = (int)Math.Round(src.Length * (double)dstRate / srcRate);
        var dst = new float[dstLen];
        double step = (double)srcRate / dstRate;
        double pos = 0;
        for (int i = 0; i < dstLen; i++) { dst[i] = SampleCubic(src, src.Length, pos); pos += step; }
        return dst;
    }

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
}
