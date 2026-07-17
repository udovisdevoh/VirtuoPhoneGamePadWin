using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VirtuoPhone.Config;

/// <summary>Everything the app persists: the preset library + which is active, the instrument, the audio
/// settings, and the controller map. Serialized as human-readable JSON (see <see cref="ConfigStore"/>).</summary>
public sealed class AppConfig
{
    public List<Preset> Presets { get; set; } = new();
    public int ActivePreset { get; set; }
    public int Instrument { get; set; }              // instrument index the Select button cycles from
    public int OctaveShift { get; set; }             // global transpose in octaves (−2..+2)
    public AudioSettings Audio { get; set; } = new();
    public ControllerMap Controls { get; set; } = new();

    public static AppConfig Default() => new() { Presets = { Preset.Default() } };

    /// <summary>Repair a freshly-loaded config so it is always usable (indices in range, cells filled in).</summary>
    public void Normalize()
    {
        if (Presets.Count == 0) Presets.Add(Preset.Default());
        foreach (var p in Presets) p.Normalize();
        ActivePreset = Math.Clamp(ActivePreset, 0, Presets.Count - 1);
        OctaveShift = Math.Clamp(OctaveShift, -2, 2);
        Audio ??= new AudioSettings();
        Controls ??= new ControllerMap();
        Controls.Normalize();
    }

    public Preset ActivePresetOrDefault() =>
        Presets.Count > 0 ? Presets[Math.Clamp(ActivePreset, 0, Presets.Count - 1)] : Preset.Default();
}

/// <summary>Loads/saves <see cref="AppConfig"/> as JSON under %APPDATA%/VirtuoPhone/config.json.</summary>
public static class ConfigStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Directory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VirtuoPhone");

    public static string FilePath => Path.Combine(Directory, "config.json");

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(FilePath), Options);
                if (cfg != null) { cfg.Normalize(); return cfg; }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[config] load failed ({e.Message}); using defaults.");
        }
        var def = AppConfig.Default();
        def.Normalize();
        return def;
    }

    public static void Save(AppConfig config)
    {
        System.IO.Directory.CreateDirectory(Directory);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(config, Options));
    }
}
