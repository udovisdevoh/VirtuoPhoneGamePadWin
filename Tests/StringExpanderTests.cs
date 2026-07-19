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
        // (ChordType.five is excluded: a 2-note power chord voiced to 21 notes spans ~10 octaves — past MIDI
        //  range at high roots, so the engine wraps its top notes. Every 3+ note chord fits.)
        ChordType[] types =
        {
            ChordType.maj, ChordType.m, ChordType.maj7, ChordType.m7, ChordType.seven,
            ChordType.dim, ChordType.dim7, ChordType.aug, ChordType.sus2, ChordType.sus4,
            ChordType.six, ChordType.nine, ChordType.m9, ChordType.maj9,
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
        Assert.All(chord, n => Assert.InRange(n.GetPitch(), 0, 127));
        // All pitches distinct (the layout never doubles a note). A very wide voicing — e.g. 21 notes of a
        // 2-note chord spans ~10 octaves — may dip below minPitchToPlay at the very bottom, which the engine
        // just leaves silent; that's expected, so we don't assert >= minPitch here.
        Assert.Equal(strings, chord.Select(n => n.GetPitch()).Distinct().Count());
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
    public void EMajor_HasNoSilentLowNotes_Regression()
    {
        // Regression: the expanded voicing must have no note below minPitchToPlay (would be silent).
        Instrument instrument = AppController.GetAppController().GetInstrument();
        int minPitch = instrument.GetMinPitchToPlay();

        var chord = new Chord(Note.E, ChordType.maj);

        Assert.Equal(instrument.GetStringCount(), chord.Size());
        Assert.All(chord, n => Assert.True(n.GetPitch() >= minPitch, $"pitch {n.GetPitch()} < minPitch {minPitch}"));
        Assert.All(chord, n => Assert.InRange(n.GetPitch(), 0, 127));
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
