using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class StringExpander
{
    private static Dictionary<int, int> listNoteTypeCount = new Dictionary<int, int>();

    public static void AddMoreNotes(Chord chord, int desiredStringCount)
    {
        // Distinct pitch classes present, and the current highest pitch.
        var classes = new HashSet<int>();
        int max = int.MinValue;
        foreach (Note n in chord)
        {
            classes.Add(((n.GetPitch() % 12) + 12) % 12);
            if (n.GetPitch() > max) max = n.GetPitch();
        }

        // Stack the next distinct chord tone ABOVE the current top — tight ascending, so N notes span only
        // ~ceil(N / classCount) octaves. (The old rarest-note stacking octave-jumped and ran away — 21 notes of
        // a 3-note chord spanned ~11 octaves, past MIDI range.) A final pass fits the whole voicing into [0,127].
        while (chord.Count < desiredStringCount)
        {
            do { max++; } while (!classes.Contains(((max % 12) + 12) % 12));
            chord.AddNote(new Note(max));
        }

        FitToRange(chord);
    }

    public static void RemoveSomeNotes(Chord chord, int desiredStringCount)
    {
        int stringCountToRemove = chord.Count - desiredStringCount;

        int averagePitch = chord.GetAveragePitch();

        while (stringCountToRemove > 0)
        {
            int mostFrequentNoteType = GetMostFrequentNote(chord);
            Note? mostExtremeNote = GetMostExtremeNote(chord, mostFrequentNoteType, averagePitch);
            if (mostExtremeNote != null)
            {
                chord.Remove(mostExtremeNote);
            }
            stringCountToRemove--;
        }
    }

    private static Note? GetMostExtremeNote(Chord chord, int noteType, int averagePitch)
    {
        Note? mostExtremeNote = null;
        int largestDifference = -1;
        int currentDifference;

        foreach (Note note in chord)
        {
            if (note.GetPitch() % 12 == noteType)
            {
                currentDifference = Math.Abs(note.GetPitch() - averagePitch);
                if (currentDifference > largestDifference)
                {
                    mostExtremeNote = note;
                    largestDifference = currentDifference;
                }
            }
        }

        return mostExtremeNote;
    }

    private static int GetRarestNote(Chord chord)
    {
        int rarestNote = 0;

        lock (listNoteTypeCount)
        {
            listNoteTypeCount.Clear();
            foreach (Note note in chord)
            {
                if (note != null)
                {
                    int notePitch = note.GetPitch() % 12;
                    if (listNoteTypeCount.ContainsKey(notePitch))
                    {
                        listNoteTypeCount[notePitch] = listNoteTypeCount[notePitch] + 1;
                    }
                    else
                    {
                        listNoteTypeCount[notePitch] = 1;
                    }
                }
            }

            int rarestCount = -1;
            foreach (int pitch in listNoteTypeCount.Keys)
            {
                int count = listNoteTypeCount[pitch];

                if (rarestCount == -1 || count < rarestCount)
                {
                    rarestCount = count;
                    rarestNote = pitch;
                }
            }
        }

        return rarestNote;
    }

    private static int GetMostFrequentNote(Chord chord)
    {
        int mostFrequentNote = 0;

        lock (listNoteTypeCount)
        {
            listNoteTypeCount.Clear();
            foreach (Note note in chord)
            {
                if (note != null)
                {
                    int notePitch = note.GetPitch() % 12;
                    if (listNoteTypeCount.ContainsKey(notePitch))
                    {
                        listNoteTypeCount[notePitch] = listNoteTypeCount[notePitch] + 1;
                    }
                    else
                    {
                        listNoteTypeCount[notePitch] = 1;
                    }
                }
            }

            int mostFrequentCount = -1;
            foreach (int pitch in listNoteTypeCount.Keys)
            {
                int count = listNoteTypeCount[pitch];

                if (count > mostFrequentCount)
                {
                    mostFrequentCount = count;
                    mostFrequentNote = pitch;
                }
            }
        }

        return mostFrequentNote;
    }

    private static void ReduceOctaveAllNotes(Chord chord)
    {
        foreach (Note note in chord)
        {
            note.SetPitch(note.GetPitch() - 12);
        }
    }

    private static void RaiseOctaveAllNotes(Chord chord)
    {
        foreach (Note note in chord)
        {
            note.SetPitch(note.GetPitch() + 12);
        }
    }

    // Octave-shift the whole voicing into the wide pitch domain [Note.MinPitch, Note.MaxPitch]. This is NOT the
    // MIDI range: a 48-note voicing of a triad spans ~16 octaves, so squeezing it into 0-127 was impossible and
    // the two loops fought each other. Notes that end up outside the sample table simply don't sound.
    private static void FitToRange(Chord chord)
    {
        int min = int.MaxValue, max = int.MinValue;
        foreach (Note n in chord) { min = Math.Min(min, n.GetPitch()); max = Math.Max(max, n.GetPitch()); }
        while (max > Note.MaxPitch) { ReduceOctaveAllNotes(chord); max -= 12; min -= 12; }
        while (min < Note.MinPitch) { RaiseOctaveAllNotes(chord);  min += 12; max += 12; }
    }
}
