using System;
using NAudio.Wave;

namespace VirtuoPhone;

/// <summary>
/// One playing voice as an NAudio <see cref="ISampleProvider"/>, designed to be an input of a
/// <c>MixingSampleProvider</c>. Reads a cached mono sample at a variable rate (pitch), applies an optional
/// attack/release envelope and a glissando, and outputs stereo with per-side gain. Returns 0 (end of stream)
/// once finished, so the mixer removes it automatically.
///
/// We deliberately do NOT hand-roll the mixing/output stage anymore — NAudio's mixer owns that. This class
/// is only the per-voice sample reader (pitch, envelope, glissando) that a real SoundPool needs.
/// </summary>
internal sealed class VoiceSampleProvider : ISampleProvider
{
    private readonly float[] src;
    private readonly double rateRatio;     // sourceRate / outputRate (folded resampling)
    private readonly bool loop;
    private readonly float attackInc, releaseInc;
    private readonly int outputSampleRate;
    private readonly double baseRate;      // the rate the voice was struck at — the glissando anchor (see GlideRate)

    private double pos;
    private double pitch = 1.0, targetPitch = 1.0, pitchGlideInc;
    private float volL, volR;
    private float gain;
    private volatile bool releasing;
    private bool finished;

    public int StreamId { get; }
    public WaveFormat WaveFormat { get; }

    public VoiceSampleProvider(int streamId, float[] src, int sourceRate, int outputRate, float leftVolume,
                               float rightVolume, bool loop, float rate, float attackSeconds, float releaseSeconds)
    {
        StreamId = streamId;
        this.src = src;
        outputSampleRate = outputRate;
        rateRatio = (double)sourceRate / outputRate;
        this.loop = loop;
        volL = leftVolume;
        volR = rightVolume;
        pitch = targetPitch = rate <= 0f ? 1.0 : rate;
        baseRate = pitch;
        attackInc = attackSeconds > 0f ? 1f / (attackSeconds * outputRate) : 0f;
        releaseInc = releaseSeconds > 0f ? 1f / (releaseSeconds * outputRate) : 0f;
        gain = attackInc > 0f ? 0f : 1f;    // start silent only when there's an attack
        WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(outputRate, 2);
    }

    public void SetVolume(float left, float right) { volL = left; volR = right; }

    public void SetRate(float rate)
    {
        pitch = targetPitch = rate <= 0f ? 1.0 : rate;
        pitchGlideInc = 0;
    }

    /// <summary>Glide (portamento) to <c>baseRate × factor</c> over <paramref name="seconds"/> — anchored to
    /// the rate the voice was STRUCK at, so chained glides re-target from a fixed base and never drift
    /// (e.g. a note going E→F→E returns exactly to E).</summary>
    public void GlideRate(float factor, float seconds) => StartGlide(baseRate * factor, seconds);

    /// <summary>Glide from the current rate to an absolute <paramref name="targetRate"/> (used by the drone).</summary>
    public void GlideToRate(float targetRate, float seconds)
        => StartGlide(targetRate <= 0f ? pitch : targetRate, seconds);

    private void StartGlide(double target, float seconds)
    {
        if (seconds <= 0f) { pitch = target; pitchGlideInc = 0; }
        else pitchGlideInc = (target - pitch) / (seconds * outputSampleRate);
        targetPitch = target;
    }

    public void Release() => releasing = true;

    public int Read(float[] buffer, int offset, int count)
    {
        if (finished) return 0;

        int srcLen = src.Length;
        int frames = count / 2;
        int o = offset;
        int produced = 0;

        for (int f = 0; f < frames; f++)
        {
            if (pos >= srcLen)
            {
                if (loop) pos %= srcLen;
                else { finished = true; break; }
            }

            // Portamento.
            if (pitchGlideInc != 0.0)
            {
                pitch += pitchGlideInc;
                if ((pitchGlideInc > 0.0 && pitch >= targetPitch) || (pitchGlideInc < 0.0 && pitch <= targetPitch))
                {
                    pitch = targetPitch;
                    pitchGlideInc = 0.0;
                }
            }

            // Envelope: release ramp (instant when none), else attack ramp to unity.
            if (releasing)
            {
                if (releaseInc <= 0f) { finished = true; break; }
                gain -= releaseInc;
                if (gain <= 0f) { finished = true; break; }
            }
            else if (gain < 1f && attackInc > 0f)
            {
                gain += attackInc;
                if (gain > 1f) gain = 1f;
            }

            float s = SampleCubic(src, srcLen, pos) * gain;
            buffer[o++] = s * volL;
            buffer[o++] = s * volR;
            pos += pitch * rateRatio;
            produced += 2;
        }

        return produced;   // 0 once finished → the mixer drops this voice
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
}
