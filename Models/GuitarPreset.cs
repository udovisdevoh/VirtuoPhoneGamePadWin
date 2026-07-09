using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class GuitarPreset : IEnumerable<Chord>
{
    private string name = string.Empty;

    private List<Chord> chordList = new List<Chord>();

    /**
    * Create new emptu preset
    */
    public GuitarPreset()
    {
        name = "Untitled";
    }

    /**
    * Create preset from serialized string
    * @param serialized preset as string
    */
    public GuitarPreset(string serialized)
    {
        serialized = serialized.Replace('\r', '\n');
        string[] lineList = serialized.Split("\n");

        name = lineList[0].Trim();

        for (int i = 1; i < lineList.Length; i++)
    {
            string chordAsString = lineList[i].Trim();
            if (chordAsString.Length > 1)
            {
                Chord chord = new Chord(chordAsString);
                chordList.Add(chord);
            }
        }
    }

    /**
    * @return custom name for preset
    */
    public string GetName()
    {
        return name;
    }

    /**
    * @param name custom name for preset
    */
    public void SetName(string name)
    {
        this.name = name;
    }

    public void SetChord(int index, Chord chord)
    {
        while (index >= chordList.Count)
        chordList.Add(new Chord(Note.E, ChordType.m));
        chordList[index] = chord;
    }

    public IEnumerator<Chord> GetEnumerator()
    {
        return chordList.GetEnumerator();
    }

    public int Size()
    {
        return chordList.Count;
    }

    public Note getNoteAt(Point coordinates)
    {
        Chord chord = chordList[coordinates.Y];
        Note note = chord[coordinates.X];
        return note;
    }

    public int GetChordFundamentalNoteTypeAt(Point coordinates)
    {
        Chord chord = chordList[coordinates.Y];
        Note note = chord[0];
        return note.GetPitch() % 12;
    }

    public string Serialize()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append(name.Trim());
        stringBuilder.Append('\r');

        foreach (Chord chord in chordList)
        {
            if (chord != null)
            {
                stringBuilder.Append(chord.Serialize().Trim());
                stringBuilder.Append('\r');
            }
        }

        return stringBuilder.ToString();
    }

    public void replaceChord(Chord oldChord, Chord newChord)
    {
        int index = chordList.IndexOf(oldChord);
        chordList[index] = newChord;
    }

    public void removeChord(Chord chord)
    {
        chordList.Remove(chord);
    }

    public void add(Chord chord)
    {
        chordList.Add(chord);
    }

    public void regenerateChords()
    {
        int index = 0;
        foreach (Chord chord in chordList)
        {
            Chord newChord = new Chord(chord.GetNoteType(), Chord.getChordTypeFromId(chord.GetChordTypeId()));
            chordList[index] = newChord;
            index++;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
