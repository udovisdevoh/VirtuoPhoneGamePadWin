using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtuoPhone.Models;

/// <summary>
/// Voice leading: re-voice a chord as the voicing <b>closest</b> to a reference voicing (minimal total
/// semitone movement, per position), subject to two hard rules:
/// <list type="number">
/// <item>every chord tone (target pitch class) appears at least once, and</item>
/// <item>no two <b>consecutive</b> voices are the exact same pitch — the same note may repeat only in a
/// <i>different octave</i>.</item>
/// </list>
///
/// Picking each position independently (nearest tone) can force identical neighbours; "fixing" that locally by
/// octave-spacing pushes voices far from the target. Instead this <b>evaluates every nearby inversion</b> with a
/// small dynamic program over the positions: for each position it considers the chord tones within an octave-ish
/// window, and finds the assignment of <b>minimal total movement</b> that never repeats a pitch on adjacent
/// voices and covers all tones. Ties prefer the higher pitch. Compute once per preset and cache.
///
/// Examples — centre E major <c>E B E G# B E G# B</c> → C major = <c>E C E G C E G C</c> (each note 0/±1);
/// E → Bb = <c>F A# F A# D F A# D</c> (no adjacent <c>A# A#</c>), not an octave-spaced voicing.
/// </summary>
public static class VoiceLeading
{
    private const int Window = 13;   // consider chord tones within ±13 semitones of each source voice

    /// <summary>
    /// The closest voicing (per-position minimal movement) whose voices are all target chord tones, with every
    /// target class present and no two consecutive voices sharing the exact same pitch.
    /// </summary>
    public static int[] ClosestVoicing(IReadOnlyList<int> sourceVoicing, IReadOnlyCollection<int> targetPitchClasses)
    {
        int n = sourceVoicing.Count;
        int[] classes = targetPitchClasses.Select(PitchClass).Distinct().ToArray();
        var classIndex = new Dictionary<int, int>();
        for (int j = 0; j < classes.Length; j++) classIndex[classes[j]] = j;
        int fullMask = (1 << classes.Length) - 1;

        // Candidate chord-tone pitches per position (within the window; always non-empty).
        var cand = new List<int>[n];
        for (int i = 0; i < n; i++) cand[i] = Candidates(sourceVoicing[i], classes);

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
                    if (p == prevPitch) continue;                                  // rule 2: no adjacent duplicate
                    var key = (ci, pkey.mask | (1 << classIndex[PitchClass(p)]));   // rule 1 tracked via the mask
                    long c = pval.cost + Cost(p, sourceVoicing[i]);
                    if (!dp[i].TryGetValue(key, out var e) || c < e.cost) dp[i][key] = (c, pkey.cand, pkey.mask);
                }
            }
        }

        // Cheapest end state that covers every chord tone; fall back to the cheapest overall if (n < classes)
        // makes completeness impossible.
        (int cand, int mask) best = default;
        long bestCost = long.MaxValue;
        foreach (var (key, val) in dp[n - 1])
            if (key.mask == fullMask && val.cost < bestCost) { bestCost = val.cost; best = key; }
        if (bestCost == long.MaxValue)
            foreach (var (key, val) in dp[n - 1])
                if (val.cost < bestCost) { bestCost = val.cost; best = key; }

        var result = new int[n];
        var cur = best;
        for (int i = n - 1; i >= 0; i--)
        {
            result[i] = cand[i][cur.cand];
            if (i > 0) { var v = dp[i][cur]; cur = (v.pCand, v.pMask); }
        }
        return result;
    }

    /// <summary>Chord-tone pitches within the window around <paramref name="source"/> (candidates for one voice).</summary>
    private static List<int> Candidates(int source, int[] classes)
    {
        var list = new List<int>();
        foreach (int c in classes)
        {
            int nearest = NearestPitchForClass(source, c);          // always within ±6, so always included
            for (int oct = -2; oct <= 2; oct++)
            {
                int p = nearest + oct * 12;
                if (p >= 0 && p <= 127 && Math.Abs(p - source) <= Window) list.Add(p);
            }
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
