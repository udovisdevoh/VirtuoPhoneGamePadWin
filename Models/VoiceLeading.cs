using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtuoPhone.Models;

/// <summary>
/// Voice leading: re-voice a chord as the inversion <b>closest</b> to a reference voicing, minimising the
/// total absolute semitone movement per position, <b>while keeping every chord tone present</b>.
///
/// Example — centre E major <c>E B E G# B E G# B</c> → C major becomes <c>E C E G C E G C</c> (each note
/// moves 0/±1), not a full transpose <c>C G C E G C E G</c>.
///
/// Positions are independent (the total |Δ| is separable), so each first takes the nearest pitch whose
/// class is in the target chord; then any missing chord tone is added at the cheapest position (whose class
/// is over-represented, so nothing is lost). Compute once per preset and cache — do not recompute on every
/// chord change (the play harness pre-renders one voicing per grid cell).
/// </summary>
public static class VoiceLeading
{
    /// <summary>
    /// Re-voice: each position takes the nearest pitch (any octave) whose pitch class is in
    /// <paramref name="targetPitchClasses"/>; then every target class is guaranteed to appear at least once.
    /// Result has the same length as <paramref name="sourceVoicing"/> (which must be ≥ the class count).
    /// </summary>
    public static int[] ClosestVoicing(IReadOnlyList<int> sourceVoicing, IReadOnlyCollection<int> targetPitchClasses)
    {
        var result = new int[sourceVoicing.Count];
        for (int i = 0; i < sourceVoicing.Count; i++)
            result[i] = NearestPitch(sourceVoicing[i], targetPitchClasses);

        EnsureAllChordTones(result, sourceVoicing, targetPitchClasses);
        return result;
    }

    /// <summary>
    /// Nearest pitch to <paramref name="source"/> whose pitch class (0-11) is in <paramref name="classes"/>.
    /// Equidistant candidates prefer the higher pitch (up).
    /// </summary>
    public static int NearestPitch(int source, IReadOnlyCollection<int> classes)
    {
        int best = source;
        int bestDist = int.MaxValue;
        foreach (int pc in classes)
        {
            int candidate = NearestPitchForClass(source, pc);
            int dist = Math.Abs(candidate - source);
            if (dist < bestDist || (dist == bestDist && candidate > best))
            {
                bestDist = dist;
                best = candidate;
            }
        }
        return best;
    }

    /// <summary>Nearest pitch to <paramref name="source"/> with the given pitch class (up wins the ±6 tie).</summary>
    public static int NearestPitchForClass(int source, int pitchClass)
    {
        int up = (((pitchClass - source) % 12) + 12) % 12;
        return up <= 6 ? source + up : source + up - 12;
    }

    /// <summary>
    /// Make sure every target class appears in <paramref name="voicing"/>. For each missing class, convert
    /// the position that costs the least extra movement — but only positions whose current class is
    /// over-represented, so we never drop a chord tone. (No-op when the voicing is already complete.)
    /// </summary>
    private static void EnsureAllChordTones(int[] voicing, IReadOnlyList<int> source, IReadOnlyCollection<int> classes)
    {
        var count = new Dictionary<int, int>();
        foreach (int p in voicing)
        {
            int c = PitchClass(p);
            count[c] = count.GetValueOrDefault(c) + 1;
        }

        foreach (int missing in classes.Where(c => count.GetValueOrDefault(c) == 0).OrderBy(c => c))
        {
            int bestPos = -1, bestPitch = 0, bestExtra = int.MaxValue;
            for (int i = 0; i < voicing.Length; i++)
            {
                if (count[PitchClass(voicing[i])] <= 1) continue;   // don't remove the last of its class
                int candidate = NearestPitchForClass(source[i], missing);
                int extra = Math.Abs(candidate - source[i]) - Math.Abs(voicing[i] - source[i]);
                if (extra < bestExtra)
                {
                    bestExtra = extra;
                    bestPos = i;
                    bestPitch = candidate;
                }
            }

            if (bestPos >= 0)
            {
                count[PitchClass(voicing[bestPos])]--;
                voicing[bestPos] = bestPitch;
                count[missing] = count.GetValueOrDefault(missing) + 1;
            }
        }
    }

    private static int PitchClass(int pitch) => ((pitch % 12) + 12) % 12;
}
