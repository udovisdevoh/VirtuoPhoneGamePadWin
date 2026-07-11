using System.Collections.Generic;
using System.Linq;
using VirtuoPhone;
using VirtuoPhone.Models;
using Xunit;

namespace VirtuoPhone.Tests;

/// <summary>
/// Covers the chord/scale types added where the old <c>#warning ajouter ces types</c> used to be — the two
/// altered suspends, the seven diatonic modes, and the harmonic/melodic-minor &amp; exotic scales. Each must
/// build to the instrument's string count, yield exactly the intended pitch classes at every root, stay
/// playable, and round-trip through serialization. Forces the silent <see cref="DummySoundPool"/> so no audio
/// device is opened.
/// </summary>
public class ChordTypeTests
{
    static ChordTypeTests()
    {
        AudioBackend.Factory = polyphony => new DummySoundPool(polyphony);
    }

    // Intervals (semitones from the root) each new type is supposed to contain.
    private static readonly Dictionary<ChordType, int[]> Expected = new()
    {
        [ChordType.sus4sharp]             = new[] { 0, 6, 7 },
        [ChordType.sus2flat]              = new[] { 0, 1, 7 },
        [ChordType.ionian]                = new[] { 0, 2, 4, 5, 7, 9, 11 },
        [ChordType.dorian]                = new[] { 0, 2, 3, 5, 7, 9, 10 },
        [ChordType.phrygian]              = new[] { 0, 1, 3, 5, 7, 8, 10 },
        [ChordType.lydian]                = new[] { 0, 2, 4, 6, 7, 9, 11 },
        [ChordType.mixolydian]            = new[] { 0, 2, 4, 5, 7, 9, 10 },
        [ChordType.aeolian]               = new[] { 0, 2, 3, 5, 7, 8, 10 },
        [ChordType.locrian]               = new[] { 0, 1, 3, 5, 6, 8, 10 },
        [ChordType.harmonic_minor]        = new[] { 0, 2, 3, 5, 7, 8, 11 },
        [ChordType.phrygian_dominant]     = new[] { 0, 1, 4, 5, 7, 8, 10 },
        [ChordType.melodic_minor]         = new[] { 0, 2, 3, 5, 7, 9, 11 },
        [ChordType.mixolydian_b6]         = new[] { 0, 2, 4, 5, 7, 8, 10 },
        [ChordType.lydian_dominant]       = new[] { 0, 2, 4, 6, 7, 9, 10 },
        [ChordType.double_harmonic_major] = new[] { 0, 1, 4, 5, 7, 8, 11 },
        [ChordType.double_harmonic_minor] = new[] { 0, 2, 3, 6, 7, 8, 11 },
        [ChordType.dorian_sharp_4]        = new[] { 0, 2, 3, 6, 7, 9, 10 },
    };

    public static IEnumerable<object[]> Cases()
    {
        for (int root = 0; root < 12; root++)
            foreach (ChordType t in Expected.Keys)
                yield return new object[] { root, t };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void NewType_YieldsExpectedPitchClasses_AndIsPlayable(int root, ChordType type)
    {
        Instrument instrument = AppController.GetAppController().GetInstrument();
        int strings = instrument.GetStringCount();
        int minPitch = instrument.GetMinPitchToPlay();

        var chord = new Chord(root, type);

        Assert.Equal(strings, chord.Size());

        // The voicing/expander only transposes and duplicates note types, so the pitch classes present must be
        // exactly the scale/chord tones transposed to the requested root — no more, no fewer.
        var expected = Expected[type].Select(i => (i + root) % 12).ToHashSet();
        var actual = chord.Select(n => n.GetPitch() % 12).ToHashSet();
        Assert.Equal(expected, actual);

        Assert.All(chord, n =>
        {
            Assert.InRange(n.GetPitch(), 0, 127);
            Assert.True(n.GetPitch() >= minPitch,
                $"{type} root {root}: pitch {n.GetPitch()} < minPitchToPlay {minPitch} (note would be silent)");
        });
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void NewType_HasSerializationId_ThatRoundTrips(int root, ChordType type)
    {
        var chord = new Chord(root, type);

        int id = chord.GetChordTypeId();
        Assert.NotEqual(-1, id);
        Assert.Equal(type, Chord.getChordTypeFromId(id));

        // Full text serialize → deserialize preserves the pitch classes.
        var restored = new Chord(chord.Serialize());
        Assert.Equal(
            chord.Select(n => n.GetPitch() % 12).ToHashSet(),
            restored.Select(n => n.GetPitch() % 12).ToHashSet());
    }

    [Fact]
    public void EveryChordType_BuildsWithoutFallingToDefault()
    {
        // Guards against a future enum value being added without a voicing (which would hit the switch default,
        // leave noteList empty, and throw). Every declared ChordType must produce a non-empty, id-mapped chord.
        AppController.GetAppController().GetInstrument();
        foreach (ChordType type in System.Enum.GetValues<ChordType>())
        {
            var chord = new Chord(Note.C, type);
            Assert.True(chord.Size() > 0, $"{type} produced an empty chord (missing voicing?)");
            Assert.NotEqual(-1, chord.GetChordTypeId());
        }
    }
}
