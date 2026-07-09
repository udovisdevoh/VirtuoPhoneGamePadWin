using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;



/**
* To add more notes to the chord if the guitar has more than 6 strings
* @author Guillaume Lacasse
*/
public class StringExpander
{
    private static TreeMap<Integer, Integer> listNoteTypeCount = new TreeMap<Integer, Integer>();

    public static void addMoreNotes(Chord chord, int desiredStringCount)
    {

        int stringCountToAdd = desiredStringCount - chord.Count;

        while (stringCountToAdd > 0)
        {
            if (stringCountToAdd % 3 == 1)
            {
                reduceOctaveAllNotes(chord);
            }

            int rarestNote = getRarestNote(chord);

            Note newNote = new Note(rarestNote);

            int lastPitch = chord.lastPitch();

            while (chord.containsExact(newNote) || newNote.getPitch() <= lastPitch)
            newNote.setPitch(newNote.getPitch() + 12);

            chord.addNote(newNote);

            stringCountToAdd--;
        }
    }

    public static void removeSomeNotes(Chord chord, int desiredStringCount)
    {
        int stringCountToRemove = chord.Count - desiredStringCount;

        int averagePitch = chord.getAveragePitch();

        while (stringCountToRemove > 0)
        {
            int mostFrequentNoteType = getMostFrequentNote(chord);
            Note mostExtremeNote = getMostExtremeNote(chord, mostFrequentNoteType, averagePitch);
            chord.remove(mostExtremeNote);
            stringCountToRemove--;
        }
    }

    private static Note getMostExtremeNote(Chord chord, int noteType, int averagePitch)
    {
        Note mostExtremeNote = null;
        int largestDifference = -1;
        int currentDifference;

        for (Note note : chord)
        {
            if (note.getPitch() % 12 == noteType)
            {
                currentDifference = Math.Abs(note.getPitch() - averagePitch);
                if (currentDifference > largestDifference)
                {
                    mostExtremeNote = note;
                    largestDifference = currentDifference;
                }
            }
        }

        return mostExtremeNote;
    }

    private static int getRarestNote(Chord chord)
    {
        int rarestNote = 0;

        synchronized(listNoteTypeCount) {
            listNoteTypeCount.clear();
            for (Note note : chord)
            {
                if (note != null)
                {
                    int notePitch = note.getPitch() % 12;
                    if (listNoteTypeCount.containsKey(notePitch))
                    {
                        listNoteTypeCount.put(notePitch, listNoteTypeCount[notePitch] + 1);
                    }
                    else
                    {
                        listNoteTypeCount.put(notePitch, 1);
                    }
                }
            }

            int rarestCount = -1;
            for (int pitch : listNoteTypeCount.keySet())
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

    private static int getMostFrequentNote(Chord chord)
    {
        int mostFrequentNote = 0;

        synchronized(listNoteTypeCount) {
            listNoteTypeCount.clear();
            for (Note note : chord)
            {
                if (note != null)
                {
                    int notePitch = note.getPitch() % 12;
                    if (listNoteTypeCount.containsKey(notePitch))
                    {
                        listNoteTypeCount.put(notePitch, listNoteTypeCount[notePitch] + 1);
                    }
                    else
                    {
                        listNoteTypeCount.put(notePitch, 1);
                    }
                }
            }

            int mostFrequentCount = -1;
            for (int pitch : listNoteTypeCount.keySet())
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

    private static void reduceOctaveAllNotes(Chord chord)
    {
        for (Note note : chord)
        {
            note.setPitch(note.getPitch() - 12);
        }
    }
}
