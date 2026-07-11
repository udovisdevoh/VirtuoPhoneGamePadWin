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
}
