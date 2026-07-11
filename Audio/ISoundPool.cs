namespace VirtuoPhone;

/// <summary>
/// Low-latency, polyphonic sample player modeled on Android's <c>SoundPool</c>.
///
/// Lifecycle: <see cref="Load"/> pre-decodes a sample and returns a <c>soundId</c>;
/// <see cref="Play"/> starts an independent voice ("stream") from a loaded sound and returns a
/// <c>streamId</c> used to control that voice afterwards. Volumes are linear 0..1 gains;
/// <c>rate</c> is a playback-speed multiplier (1.0 = original pitch, 2.0 = one octave up).
///
/// This is the single seam that lets the audio backend be swapped (see <see cref="AudioBackend"/>):
/// <see cref="DummySoundPool"/> is a silent/headless implementation, <see cref="NAudioSoundPool"/>
/// is the real WASAPI engine. Keep this surface identical to the ported Java model's expectations.
/// </summary>
public interface ISoundPool
{
    /// <summary>Pre-decode a sample into memory. Returns a soundId (0 = failed / no sample).</summary>
    int Load(int resourceId, int priority);

    /// <summary>Start a voice from a loaded sound. <paramref name="loop"/> != 0 loops forever;
    /// <paramref name="attackSeconds"/>/<paramref name="releaseSeconds"/> add an optional fade in/out
    /// (0 = none — the default hard edge). Returns a streamId (0 = not playing).</summary>
    int Play(int soundId, float leftVolume, float rightVolume, int priority, int loop, float rate,
             float attackSeconds, float releaseSeconds);

    /// <summary>Convenience overload with no envelope (attack = release = 0).</summary>
    int Play(int soundId, float leftVolume, float rightVolume, int priority, int loop, float rate)
        => Play(soundId, leftVolume, rightVolume, priority, loop, rate, 0f, 0f);

    /// <summary>Stop and free a playing voice.</summary>
    void Stop(int streamId);

    /// <summary>Update a playing voice's left/right gains.</summary>
    void SetVolume(int streamId, float left, float right);

    /// <summary>Update a playing voice's playback rate (pitch), instantly.</summary>
    void SetRate(int streamId, float rate);

    /// <summary>Smoothly glide a playing voice to <c>baseRate × <paramref name="rateFactor"/></c> over
    /// <paramref name="glideSeconds"/> (0 = instant), where <c>baseRate</c> is the rate the voice was
    /// started at. Anchoring to the ORIGINAL rate (not the current, possibly mid-glide one) keeps chained
    /// glides from drifting/compounding. Used for note portamento/glissando.</summary>
    void GlideRate(int streamId, float rateFactor, float glideSeconds);

    /// <summary>Smoothly glide a playing voice from its current rate to the absolute
    /// <paramref name="targetRate"/> over <paramref name="glideSeconds"/> (0 = instant). Used by the drone,
    /// which knows the exact target rate for each chord root.</summary>
    void GlideToRate(int streamId, float targetRate, float glideSeconds);

    /// <summary>Release the engine and all resources.</summary>
    void Release();
}
