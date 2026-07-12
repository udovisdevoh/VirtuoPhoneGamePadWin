namespace VirtuoPhone.Config;

/// <summary>User-configurable audio output settings (persisted in the JSON config, consumed by
/// <see cref="NAudioSoundPool"/>). Replaces the old VP_AUDIO_MODE / VP_LATENCY_MS env vars.</summary>
public sealed class AudioSettings
{
    /// <summary>WASAPI exclusive (44.1 kHz bit-exact, no Windows volume) vs shared (default, volume works).</summary>
    public bool Exclusive { get; set; } = false;

    /// <summary>Requested output buffer latency in milliseconds (10 ms is clean; lower risks underruns).</summary>
    public int LatencyMs { get; set; } = 10;

    /// <summary>Master volume 0..1, applied on top of the automatic per-polyphony headroom.</summary>
    public float MasterVolume { get; set; } = 1.0f;

    /// <summary>Output device id (NAudio MMDevice.ID). Null/empty = the system default render device.</summary>
    public string? DeviceId { get; set; }

    public AudioSettings Clone() => new()
    {
        Exclusive = Exclusive, LatencyMs = LatencyMs, MasterVolume = MasterVolume, DeviceId = DeviceId,
    };
}
