using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using VirtuoPhone.Config;
using VirtuoPhone.Input;
using VirtuoPhone.Models;
using Xunit;

namespace VirtuoPhone.Tests;

/// <summary>The JSON config must round-trip losslessly and stay human-readable (string enum keys/values).</summary>
public class ConfigTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    [Fact]
    public void AppConfig_RoundTrips_WithReadableEnums()
    {
        var cfg = AppConfig.Default();
        cfg.Normalize();
        cfg.Presets[0].Inner[Direction.Neutral] = new Cell(Note.G, ChordType.m7);
        cfg.Presets[0].Outer[Direction.Up] = new Cell(Note.A, ChordType.dim7);
        cfg.Audio.Exclusive = true;
        cfg.Audio.LatencyMs = 15;
        cfg.Audio.MasterVolume = 0.5f;
        cfg.Controls.NoteKeys[0] = Keys.Z;
        cfg.Controls.StartButton = 9;

        string json = JsonSerializer.Serialize(cfg, Opts);
        var back = JsonSerializer.Deserialize<AppConfig>(json, Opts)!;
        back.Normalize();

        // Enum dictionary keys and values are written as names (readable), not numbers.
        Assert.Contains("\"Neutral\"", json);
        Assert.Contains("m7", json);

        Assert.Equal(9, back.Presets[0].Inner.Count);
        Assert.Equal(Note.G, back.Presets[0].Inner[Direction.Neutral].Root);
        Assert.Equal(ChordType.m7, back.Presets[0].Inner[Direction.Neutral].Type);
        Assert.Equal(ChordType.dim7, back.Presets[0].Outer[Direction.Up].Type);
        Assert.True(back.Audio.Exclusive);
        Assert.Equal(15, back.Audio.LatencyMs);
        Assert.Equal(0.5f, back.Audio.MasterVolume);
        Assert.Equal(Keys.Z, back.Controls.NoteKeys[0]);
        Assert.Equal(9, back.Controls.StartButton);
    }

    [Fact]
    public void Preset_Normalize_FillsMissingCells()
    {
        var p = new Preset { Name = "sparse" };
        p.Inner[Direction.Neutral] = new Cell(Note.C, ChordType.maj);   // only one cell set
        p.Normalize();

        Assert.Equal(9, p.Inner.Count);
        Assert.Equal(4, p.Outer.Count);
    }
}
