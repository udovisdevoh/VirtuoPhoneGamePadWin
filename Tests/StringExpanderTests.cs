using System.Collections.Generic;
using System.Linq;
using VirtuoPhone;
using VirtuoPhone.Models;
using Xunit;

namespace VirtuoPhone.Tests;

/// <summary>
/// Exercises <see cref="StringExpander"/> across many chords / roots (inversions). Building a <see cref="Chord"/>
/// creates the cached <see cref="Instrument"/>, so we force the silent <see cref="DummySoundPool"/> backend to
/// avoid opening an audio device in tests.
/// </summary>
public class StringExpanderTests
{
    static StringExpanderTests()
    {
        AudioBackend.Factory = polyphony => new DummySoundPool(polyphony);
    }

    // 12 roots × a broad set of chord types = many voicings / inversions.
    public static IEnumerable<object[]> ChordCases()
    {
        ChordType[] types =
        {
            ChordType.maj, ChordType.m, ChordType.maj7, ChordType.m7, ChordType.seven,
            ChordType.dim, ChordType.dim7, ChordType.aug, ChordType.sus2, ChordType.sus4,
            ChordType.five, ChordType.six, ChordType.nine, ChordType.m9, ChordType.maj9,
            ChordType.eleven, ChordType.thirteen, ChordType.add9,
            ChordType.pentatonic_minor, ChordType.pentatonic_major,
        };
        for (int root = 0; root < 12; root++)
            foreach (ChordType t in types)
                yield return new object[] { root, t };
    }

    [Theory]
    [MemberData(nameof(ChordCases))]
    public void BuiltChord_HasStringCountNotes_AndAllArePlayable(int root, ChordType type)
    {
        Instrument instrument = AppController.GetAppController().GetInstrument();
        int strings = instrument.GetStringCount();
        int minPitch = instrument.GetMinPitchToPlay();

        var chord = new Chord(root, type);

        Assert.Equal(strings, chord.Size());
        Assert.All(chord, n =>
        {
            Assert.InRange(n.GetPitch(), 0, 127);
            Assert.True(n.GetPitch() >= minPitch,
                $"{type} root {root}: pitch {n.GetPitch()} < minPitchToPlay {minPitch} (note would be silent)");
        });
    }

    [Theory]
    [MemberData(nameof(ChordCases))]
    public void Expansion_OnlyDuplicatesExistingNoteTypes(int root, ChordType type)
    {
        AppController.GetAppController().GetInstrument();

        var chord = new Chord(root, type);
        StringExpander.RemoveSomeNotes(chord, 3);                     // reduce to a small harmonic core
        HashSet<int> coreNoteTypes = chord.Select(n => n.GetPitch() % 12).ToHashSet();

        StringExpander.AddMoreNotes(chord, 8);                        // expand back up

        Assert.Equal(8, chord.Size());
        Assert.All(chord, n => Assert.Contains(n.GetPitch() % 12, coreNoteTypes));
    }

    [Fact]
    public void EMajor_On8Strings_HasNoSilentLowNotes_Regression()
    {
        // Regression for the 6→8 octave-drop bug that silenced the two lowest notes.
        Instrument instrument = AppController.GetAppController().GetInstrument();
        int minPitch = instrument.GetMinPitchToPlay();

        var chord = new Chord(Note.E, ChordType.maj);

        Assert.Equal(8, chord.Size());
        Assert.All(chord, n => Assert.True(n.GetPitch() >= minPitch));
    }

    [Fact]
    public void RemoveSomeNotes_ReducesToTargetCount()
    {
        AppController.GetAppController().GetInstrument();
        var chord = new Chord(Note.E, ChordType.maj);

        StringExpander.RemoveSomeNotes(chord, 4);

        Assert.Equal(4, chord.Size());
    }

    [Fact]
    public void AddMoreNotes_LargeExpansion_ReachesTargetCount()
    {
        AppController.GetAppController().GetInstrument();
        var chord = new Chord(Note.E, ChordType.maj);

        StringExpander.RemoveSomeNotes(chord, 3);
        StringExpander.AddMoreNotes(chord, 17);                       // harp-like; exercises the octave-drop path

        Assert.Equal(17, chord.Size());
    }
}
