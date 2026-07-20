using System.Collections.Generic;
using System.Linq;
using VirtuoPhone.Models;
using Xunit;

namespace VirtuoPhone.Tests;

public class VoiceLeadingTests
{
    // Reference 8-button voicings as built for the instruments.
    private static readonly int[] EMajor = { 28, 35, 40, 44, 47, 52, 56, 59 };   // E B E G# B E G# B
    private static readonly int[] CMajor = { 24, 31, 36, 40, 43, 48, 52, 55 };   // C G C E G C E G

    [Theory]
    // From E major (E B E G# B E G# B) to… (the user's worked examples):
    [InlineData(new[] { 0, 4, 7 },   new[] { 28, 36, 40, 43, 48, 52, 55, 60 })]  // C   -> E C E G C E G C
    [InlineData(new[] { 9, 1, 4 },   new[] { 28, 37, 40, 45, 49, 52, 57, 61 })]  // A   -> E C# E A C# E A C#
    [InlineData(new[] { 5, 9, 0 },   new[] { 29, 36, 41, 45, 48, 53, 57, 60 })]  // F   -> F C F A C F A C
    [InlineData(new[] { 11, 3, 6 },  new[] { 27, 35, 39, 42, 47, 51, 54, 59 })]  // B   -> D# B D# F# B D# F# B
    [InlineData(new[] { 9, 0, 4 },   new[] { 28, 36, 40, 45, 48, 52, 57, 60 })]  // Am  -> E C E A C E A C
    public void ClosestVoicing_FromEMajor(int[] targetClasses, int[] expected)
    {
        Assert.Equal(expected, VoiceLeading.ClosestVoicing(EMajor, new HashSet<int>(targetClasses)));
    }

    [Fact]
    public void ClosestVoicing_CMajor_To_EMajor()
    {
        // C major (C G C E G C E G) -> E major becomes B G# B E G# B E G#.
        int[] expected = { 23, 32, 35, 40, 44, 47, 52, 56 };
        Assert.Equal(expected, VoiceLeading.ClosestVoicing(CMajor, new HashSet<int> { 4, 8, 11 }));
    }

    [Fact]
    public void ClosestVoicing_SameChord_IsUnchanged()
    {
        Assert.Equal(EMajor, VoiceLeading.ClosestVoicing(EMajor, new HashSet<int> { 4, 8, 11 }));
    }

    [Theory]
    [InlineData(35, new[] { 9, 1, 4 }, 37)]   // B: tie A(33)/C#(37) -> prefer the higher (C#)
    [InlineData(59, new[] { 9, 1, 4 }, 61)]   // B (higher octave): same tie -> C#
    [InlineData(28, new[] { 0, 4, 7 }, 28)]   // E is already a chord tone -> unchanged
    [InlineData(47, new[] { 0, 4, 7 }, 48)]   // B -> C (up 1)
    [InlineData(44, new[] { 0, 4, 7 }, 43)]   // G# -> G (down 1)
    [InlineData(0,  new[] { 6 },       6)]    // C -> F#: ±6 tie -> prefer up
    public void NearestPitch_Cases(int source, int[] classes, int expected)
    {
        Assert.Equal(expected, VoiceLeading.NearestPitch(source, new HashSet<int>(classes)));
    }

    [Fact]
    public void NearestPitch_NeverMovesMoreThanSixSemitones()
    {
        var classes = new HashSet<int> { 0, 4, 7 };
        for (int source = 0; source < 128; source++)
        {
            int p = VoiceLeading.NearestPitch(source, classes);
            Assert.True(System.Math.Abs(p - source) <= 6, $"{source} -> {p}");
            Assert.Contains(((p % 12) + 12) % 12, classes);
        }
    }

    [Fact]
    public void ClosestVoicing_EveryMajorTriad_ContainsAllThreeTones()
    {
        for (int root = 0; root < 12; root++)
        {
            var classes = new HashSet<int> { root, (root + 4) % 12, (root + 7) % 12 };
            var present = VoiceLeading.ClosestVoicing(EMajor, classes)
                .Select(p => ((p % 12) + 12) % 12).ToHashSet();
            foreach (int c in classes)
                Assert.Contains(c, present);
        }
    }

    [Fact]
    public void ClosestVoicing_AddsMissingChordTones()
    {
        // Adversarial: the naive nearest would map every position to C; completeness must still add the rest.
        int[] source = { 0, 0, 0, 0, 0, 0, 0, 0 };
        var present = VoiceLeading.ClosestVoicing(source, new HashSet<int> { 0, 1, 11 })
            .Select(p => ((p % 12) + 12) % 12).ToHashSet();
        foreach (int c in new[] { 0, 1, 11 })
            Assert.Contains(c, present);
    }

    // ---- No two CONSECUTIVE voices may be the exact same pitch (same note class in another octave is fine) ----

    [Fact]
    public void ClosestVoicing_EToBb_NoConsecutiveDuplicate()
    {
        // E major -> Bb/A# major {A#,D,F}. The naive nearest gave adjacent A#3 A#3 / A#4 A#4; the global search
        // finds the closest inversion with no adjacent duplicate: F A# F A# D F A# D (each voice within ±3).
        int[] v = VoiceLeading.ClosestVoicing(EMajor, new HashSet<int> { 10, 2, 5 });
        AssertStrictlyAscending(v);
        AssertContainsClasses(v, 10, 2, 5);
        Assert.Equal(new[] { 29, 34, 41, 46, 50, 53, 58, 62 }, v);
    }

    [Fact]
    public void ClosestVoicing_EToD_NoConsecutiveDuplicate()
    {
        // E major -> D major {D,F#,A}. Naive nearest gave adjacent A3 A3 / A4 A4; closest no-duplicate
        // inversion is F# A F# A D F# A D (each voice within ±3, no octave-spacing).
        int[] v = VoiceLeading.ClosestVoicing(EMajor, new HashSet<int> { 2, 6, 9 });
        AssertStrictlyAscending(v);
        AssertContainsClasses(v, 2, 6, 9);
        Assert.Equal(new[] { 30, 33, 42, 45, 50, 54, 57, 62 }, v);
    }

    [Fact]
    public void ClosestVoicing_NoConsecutiveDuplicate_ForEveryTriadAndDim7()
    {
        foreach (int[] src in new[] { EMajor, CMajor })
            for (int root = 0; root < 12; root++)
            {
                foreach (int third in new[] { 3, 4 })   // minor + major triads
                {
                    var tri = new HashSet<int> { root, (root + third) % 12, (root + 7) % 12 };
                    var v = VoiceLeading.ClosestVoicing(src, tri);
                    AssertStrictlyAscending(v);
                    AssertContainsClasses(v, tri.ToArray());
                }
                var dim7 = new HashSet<int> { root, (root + 3) % 12, (root + 6) % 12, (root + 9) % 12 };  // dash outer cells
                var vd = VoiceLeading.ClosestVoicing(src, dim7);
                AssertStrictlyAscending(vd);
                AssertContainsClasses(vd, dim7.ToArray());
            }
    }

    // ---- Same note-count → re-voice (close, ascending);  different count → skip, keep the target's voicing ----

    // Reference E minor pentatonic (E G A B D) as an 8-voice source: E G A B D E G A.
    private static readonly int[] EPentMinor = { 28, 31, 33, 35, 38, 40, 43, 45 };

    [Fact]
    public void ClosestVoicing_EPentMinor_ToDPentMinor_SameCount_IsCloseAndAscending()
    {
        // E minor pentatonic (5 notes) → D minor pentatonic (5 notes): same count, so it IS re-voiced — and
        // lands very close (the two share G/A/D), strictly ascending: F G A C D F G A.
        int[] v = VoiceLeading.ClosestVoicing(EPentMinor, new HashSet<int> { 2, 5, 7, 9, 0 });
        AssertVoicingRules(v, 2, 5, 7, 9, 0);
        Assert.Equal(new[] { 29, 31, 33, 36, 38, 41, 43, 45 }, v);
    }

    [Fact]
    public void VoiceCell_SameNoteCount_DelegatesToClosestVoicing()
    {
        // E min pent (5) → D min pent (5): equal counts → VoiceCell re-voices (same as ClosestVoicing).
        int[] dPentMinVoicing = { 26, 29, 31, 33, 36, 38, 41, 45 };   // some D-min-pent voicing (D F G A C …)
        int[] v = VoiceLeading.VoiceCell(EPentMinor, dPentMinVoicing);
        Assert.Equal(VoiceLeading.ClosestVoicing(EPentMinor, new HashSet<int> { 2, 5, 7, 9, 0 }), v);
        AssertVoicingRules(v, 2, 5, 7, 9, 0);
    }

    [Theory]
    // Different note count → the re-organisation is SKIPPED; VoiceCell keeps the target chord's own voicing
    // (sorted ascending), never the awkward remapped one. Centre E min pent (5 notes) vs a triad target (3).
    [InlineData(new[] { 40, 28, 35, 44, 32, 56, 47, 52 }, new[] { 28, 32, 35, 40, 44, 47, 52, 56 })]  // → E major
    [InlineData(new[] { 45, 33, 37, 40, 61, 49, 57, 52 }, new[] { 33, 37, 40, 45, 49, 52, 57, 61 })]  // → A major
    public void VoiceCell_DifferentNoteCount_KeepsTargetVoicingAscending(int[] targetVoicing, int[] expected)
    {
        int[] v = VoiceLeading.VoiceCell(EPentMinor, targetVoicing);
        Assert.Equal(expected, v);              // the target chord's own notes, sorted ascending — not re-voiced
        AssertStrictlyAscending(v);
    }

    // Asserts all three voicing rules at once: strictly ascending, every voice is a target chord tone, and
    // every target chord tone is present.
    private static void AssertVoicingRules(int[] v, params int[] classes)
    {
        var set = new HashSet<int>(classes);
        AssertStrictlyAscending(v);
        foreach (int p in v)
            Assert.Contains(((p % 12) + 12) % 12, set);
        var present = v.Select(p => ((p % 12) + 12) % 12).ToHashSet();
        foreach (int c in set)
            Assert.Contains(c, present);
    }

    [Fact]
    public void ClosestVoicing_ContainsEveryTone_EvenForSevenNoteScales()
    {
        // Dense targets (whole 7-note scales used as cell chords) must STILL contain every tone, with no
        // adjacent duplicate — completeness is a hard constraint, not best-effort.
        int[][] scales =
        {
            new[] { 0, 2, 4, 5, 7, 9, 11 },  // ionian
            new[] { 0, 2, 3, 5, 7, 8, 10 },  // aeolian
            new[] { 0, 1, 4, 5, 7, 8, 11 },  // double harmonic
        };
        foreach (int[] src in new[] { EMajor, CMajor })
            for (int root = 0; root < 12; root++)
                foreach (int[] sc in scales)
                {
                    var classes = sc.Select(x => (x + root) % 12).ToHashSet();
                    var v = VoiceLeading.ClosestVoicing(src, classes);
                    AssertStrictlyAscending(v);
                    AssertContainsClasses(v, classes.ToArray());
                }
    }

    /// <summary>
    /// Regression: a 48-voice source runs well past MIDI 127 at the top. Candidates used to be clamped to
    /// 0..127, so the upper voices had NO candidate at all, the DP failed at every window, and ClosestVoicing
    /// fell back to returning the SOURCE voicing — E major stayed E major no matter which cell you selected.
    /// </summary>
    [Theory]
    [InlineData(2, 6, 9)]     // D major  — the case the user reported
    [InlineData(0, 5, 7)]     // Csus4
    [InlineData(0, 6, 7)]     // Csus4#
    [InlineData(5, 9, 0)]     // F major
    public void ClosestVoicing_WideVoicingPastMidiRange_ActuallyChangesChord(int a, int b, int c)
    {
        // 48 ascending E-major voices (E B E G# …) — the real layout, spanning ~16 octaves past 127.
        int[] eClasses = { 4, 8, 11 };
        var source = new int[48];
        int p = 28;
        for (int i = 0; i < 48; i++)
        {
            while (!eClasses.Contains(((p % 12) + 12) % 12)) p++;
            source[i] = p;
            p++;
        }

        var target = new HashSet<int> { a, b, c };
        var v = VoiceLeading.ClosestVoicing(source, target);

        Assert.NotEqual(source, v);                 // the whole point: the layout must actually change
        AssertStrictlyAscending(v);
        AssertContainsClasses(v, target.ToArray());
        Assert.All(v, x => Assert.InRange(x, Note.MinPitch, Note.MaxPitch));

        // …and it must still be genuinely voice-led all the way up, not just "some ascending stack of the right
        // notes". With candidates clamped to 127 the upper voices fell back to a crude stack and drifted up to
        // 11 semitones off the source; a working DP keeps every voice within a few semitones.
        for (int i = 0; i < v.Length; i++)
            Assert.True(System.Math.Abs(v[i] - source[i]) <= 6,
                        $"voice {i} moved {v[i] - source[i]} semitones ({source[i]} -> {v[i]}) — voice leading broke down");
    }

    private static void AssertStrictlyAscending(int[] v)
    {
        for (int i = 1; i < v.Length; i++)
            Assert.True(v[i] > v[i - 1], $"not strictly ascending at index {i}: [{string.Join(", ", v)}]");
    }

    private static void AssertContainsClasses(int[] v, params int[] classes)
    {
        var present = v.Select(p => ((p % 12) + 12) % 12).ToHashSet();
        foreach (int c in classes)
            Assert.Contains(c, present);
    }
}
