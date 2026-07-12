using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtuoPhone.Models;

/// <summary>
/// Voice leading: re-voice a chord as the voicing <b>closest</b> to a reference voicing (minimal total
/// semitone movement, per position), subject to two hard rules:
/// <list type="number">
/// <item>every chord tone (target pitch class) appears at least once, and</item>
/// <item>the voicing is <b>strictly ascending</b> (each voice higher than the last — which also guarantees no
/// two voices share a pitch, and gives a clean low→high button layout).</item>
/// </list>
///
/// A small dynamic program over the positions <b>evaluates every nearby inversion</b> (the chord tones within an
/// octave-ish window of each voice) and finds the assignment of <b>minimal total movement</b> that is strictly
/// ascending and covers all tones. Ties prefer the higher pitch. Compute once per preset and cache.
///
/// Re-voicing only makes sense when both chords have the same number of notes; when they differ, use
/// <see cref="VoiceCell"/>, which skips the re-organisation and keeps the target chord's own ascending voicing.
///
/// Examples — centre E major <c>E B E G# B E G# B</c> → C major = <c>E C E G C E G C</c> (each note 0/±1);
/// E → Bb = <c>F A# F A# D F A# D</c> (ascending pitches, no octave-spacing).
/// </summary>
public static class VoiceLeading
{
    /// <summary>
    /// The closest strictly-ascending voicing (per-position minimal movement) whose voices are all target chord
    /// tones, with every target class present. Widens the candidate window until such a voicing exists — real
    /// ascending sources succeed on the first (±13) pass; a compressed source just needs a wider span to fit an
    /// ascending run.
    /// </summary>
    public static int[] ClosestVoicing(IReadOnlyList<int> sourceVoicing, IReadOnlyCollection<int> targetPitchClasses)
    {
        int[] classes = targetPitchClasses.Select(PitchClass).Distinct().ToArray();
        for (int window = 13; window <= 120; window += 12)
        {
            int[]? solved = Solve(sourceVoicing, classes, window);
            if (solved != null) return solved;
        }
        return sourceVoicing.ToArray();   // unreachable for any real chord — never crash
    }

    /// <summary>The strict-ascending DP at a fixed candidate window; null if no complete voicing fits.</summary>
    private static int[]? Solve(IReadOnlyList<int> sourceVoicing, int[] classes, int window)
    {
        int n = sourceVoicing.Count;
        var classIndex = new Dictionary<int, int>();
        for (int j = 0; j < classes.Length; j++) classIndex[classes[j]] = j;
        int fullMask = (1 << classes.Length) - 1;

        var cand = new List<int>[n];
        for (int i = 0; i < n; i++) cand[i] = Candidates(sourceVoicing[i], classes, window);

        // DP: state = (candidate index at i, bitmask of classes covered by positions 0..i)
        //     value = (min total cost, back-pointer to the previous state).
        var dp = new Dictionary<(int cand, int mask), (long cost, int pCand, int pMask)>[n];

        dp[0] = new();
        for (int ci = 0; ci < cand[0].Count; ci++)
        {
            int p = cand[0][ci];
            var key = (ci, 1 << classIndex[PitchClass(p)]);
            long c = Cost(p, sourceVoicing[0]);
            if (!dp[0].TryGetValue(key, out var e) || c < e.cost) dp[0][key] = (c, -1, -1);
        }

        for (int i = 1; i < n; i++)
        {
            dp[i] = new();
            foreach (var (pkey, pval) in dp[i - 1])
            {
                int prevPitch = cand[i - 1][pkey.cand];
                for (int ci = 0; ci < cand[i].Count; ci++)
                {
                    int p = cand[i][ci];
                    if (p <= prevPitch) continue;                                  // rule 2: strictly ascending
                    var key = (ci, pkey.mask | (1 << classIndex[PitchClass(p)]));   // rule 1 tracked via the mask
                    long c = pval.cost + Cost(p, sourceVoicing[i]);
                    if (!dp[i].TryGetValue(key, out var e) || c < e.cost) dp[i][key] = (c, pkey.cand, pkey.mask);
                }
            }
        }

        // Cheapest end state that covers every chord tone; null if none exists at this window.
        (int cand, int mask) best = default;
        long bestCost = long.MaxValue;
        foreach (var (key, val) in dp[n - 1])
            if (key.mask == fullMask && val.cost < bestCost) { bestCost = val.cost; best = key; }
        if (bestCost == long.MaxValue) return null;

        var result = new int[n];
        var cur = best;
        for (int i = n - 1; i >= 0; i--)
        {
            result[i] = cand[i][cur.cand];
            if (i > 0) { var v = dp[i][cur]; cur = (v.pCand, v.pMask); }
        }
        return result;
    }

    /// <summary>
    /// Build one grid cell's voicing from the centre voicing and the target chord's own voicing. Re-voicing
    /// (<see cref="ClosestVoicing"/>) is applied only when the two chords have the <b>same number of distinct
    /// notes</b> — then the result stays close and ascending. When the counts differ (e.g. a triad centre vs a
    /// 7th / pentatonic / scale cell) the automatic re-organisation is <b>skipped</b> and the target chord's own
    /// voicing is used (sorted ascending), avoiding the awkward non-ascending spreads that remapping would force.
    /// </summary>
    public static int[] VoiceCell(IReadOnlyList<int> centerVoicing, IReadOnlyList<int> targetChordVoicing)
    {
        var targetClasses = targetChordVoicing.Select(PitchClass).ToHashSet();
        int centerClassCount = centerVoicing.Select(PitchClass).Distinct().Count();
        return centerClassCount == targetClasses.Count
            ? ClosestVoicing(centerVoicing, targetClasses)
            : targetChordVoicing.OrderBy(p => p).ToArray();
    }

    /// <summary>Chord-tone pitches within ±<paramref name="window"/> of <paramref name="source"/> (candidates for one voice).</summary>
    private static List<int> Candidates(int source, int[] classes, int window)
    {
        var list = new List<int>();
        foreach (int c in classes)
        {
            int nearest = NearestPitchForClass(source, c);   // within ±6, so always in range
            for (int p = nearest; p <= 127 && p - source <= window; p += 12) if (p >= 0) list.Add(p);
            for (int p = nearest - 12; p >= 0 && source - p <= window; p -= 12) list.Add(p);
        }
        return list;
    }

    /// <summary>Cost of placing <paramref name="pitch"/> at a voice whose source is <paramref name="source"/>:
    /// minimise the movement first, then prefer the higher pitch on ties.</summary>
    private static long Cost(int pitch, int source) => (long)Math.Abs(pitch - source) * 1000 - pitch;

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

    private static int PitchClass(int pitch) => ((pitch % 12) + 12) % 12;
}
