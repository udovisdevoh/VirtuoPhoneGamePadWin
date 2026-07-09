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

        int stringCountToAdd = desiredStringCount - chord.Count;

        while (stringCountToAdd > 0)
        {
            if (stringCountToAdd % 3 == 1)
            {
                ReduceOctaveAllNotes(chord);
            }

            int rarestNote = GetRarestNote(chord);

            Note newNote = new Note(rarestNote);

            int lastPitch = chord.LastPitch();

            while (chord.ContainsExact(newNote) || newNote.GetPitch() <= lastPitch)
                newNote.SetPitch(newNote.GetPitch() + 12);

            chord.AddNote(newNote);

            stringCountToAdd--;
        }
    }

    public static void RemoveSomeNotes(Chord chord, int desiredStringCount)
    {
        int stringCountToRemove = chord.Count - desiredStringCount;

        int averagePitch = chord.GetAveragePitch();

        while (stringCountToRemove > 0)
        {
            int mostFrequentNoteType = GetMostFrequentNote(chord);
            Note mostExtremeNote = GetMostExtremeNote(chord, mostFrequentNoteType, averagePitch);
            chord.Remove(mostExtremeNote);
            stringCountToRemove--;
        }
    }

    private static Note GetMostExtremeNote(Chord chord, int noteType, int averagePitch)
    {
        Note mostExtremeNote = null;
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
}
