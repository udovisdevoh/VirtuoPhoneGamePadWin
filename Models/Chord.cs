using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Chord : IEnumerable<Note>, IStringSerializable
{
    private List<Note> noteList = new List<Note>();

    private string name;

    private int noteType;

    private int chordTypeId;

    /**
    * @param noteType Use constants (A,B,C etc) defined in Note class
    * @param chordType chord type
    * @param isOpen (true: all strings are used, false: some strings may not be used)
    */
    public Chord(int noteType, ChordType chordType)
    {
        AppController appController = AppController.getAppController();
        Instrument instrument = appController.getInstrument();

        this.noteType = noteType;


        chordTypeId = getChordTypeId(chordType);
        noteType = noteType % 12;
        switch (chordType)
        {
            case add9:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.A, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.A, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.G, 5));
            break;
            case aug:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.DSharp, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.G, 5));
            break;
            case aug9:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.A, 3));
            noteList.Add(new Note(Note.F, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.DSharp, 5));
            noteList.Add(new Note(Note.A, 5));
            break;
            case eleven:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.A, 4));
            noteList.Add(new Note(Note.C, 5));
            noteList.Add(new Note(Note.F, 5));
            break;
            case five:
            if (noteType == Note.G)
            {
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.B, 4));
                noteList.Add(new Note(Note.E, 5));
            }
            else
            {
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.B, 4));
                noteList.Add(new Note(Note.E, 5));
                noteList.Add(new Note(Note.B, 5));
            }
            break;
            case m:
            if (noteType == Note.A)
            {
                noteList.Add(new Note(Note.A, 2));
                noteList.Add(new Note(Note.A, 2));
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.A, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.E, 4));
            }
            else if (noteType == Note.D)
            {
                noteList.Add(new Note(Note.D, 3));
                noteList.Add(new Note(Note.D, 3));
                noteList.Add(new Note(Note.D, 3));
                noteList.Add(new Note(Note.A, 3));
                noteList.Add(new Note(Note.D, 4));
                noteList.Add(new Note(Note.F, 4));
            }
            else if (noteType == Note.DSharp)
            {
                noteList.Add(new Note(Note.D, 3));
                noteList.Add(new Note(Note.D, 3));
                noteList.Add(new Note(Note.F, 3));
                noteList.Add(new Note(Note.A, 3));
                noteList.Add(new Note(Note.D, 4));
                noteList.Add(new Note(Note.F, 4));
            }
            else if (noteType == Note.E)
            {
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.G, 4));
                noteList.Add(new Note(Note.B, 4));
                noteList.Add(new Note(Note.E, 5));
            }
            else if (noteType == Note.C)
            {
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.DSharp, 3));
                noteList.Add(new Note(Note.G, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.DSharp, 4));
            }
            else if (noteType == Note.B || noteType == Note.ASharp || noteType == Note.CSharp)
            {
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.G, 4));
                noteList.Add(new Note(Note.B, 4));
            }
            else if (noteType == Note.GSharp)
            {
                noteList.Add(new Note(Note.GSharp, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.DSharp, 4));
                noteList.Add(new Note(Note.GSharp, 4));
                noteList.Add(new Note(Note.DSharp, 5));
                noteList.Add(new Note(Note.GSharp, 5));
            }
            else
            {
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.G, 4));
                noteList.Add(new Note(Note.B, 4));
                noteList.Add(new Note(Note.E, 5));
            }
            break;
            case m11:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.A, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.E, 5));
            break;
            case m13:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.CSharp, 5));
            noteList.Add(new Note(Note.FSharp, 5));
            break;
            case m6:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.E, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.CSharp, 5));
            noteList.Add(new Note(Note.E, 5));
            break;
            case m7:
            if (noteType == Note.D)
            {
                noteList.Add(new Note(Note.D, 2));
                noteList.Add(new Note(Note.D, 2));
                noteList.Add(new Note(Note.D, 2));
                noteList.Add(new Note(Note.A, 2));
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.F, 3));
            }
            else
            {
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.G, 4));
                noteList.Add(new Note(Note.D, 5));
                noteList.Add(new Note(Note.E, 5));
            }
            break;
            case m9:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.FSharp, 5));
            break;
            case maj:
            /*noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.G, 5));*/
            if (noteType == Note.G)
            {
                noteList.Add(new Note(Note.G, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.D, 4));
                noteList.Add(new Note(Note.G, 4));
                noteList.Add(new Note(Note.B, 4));
                noteList.Add(new Note(Note.G, 5));
            }
            else if (noteType == Note.C || noteType == Note.CSharp)
            {
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.G, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.E, 4));
            }
            else if (noteType == Note.F)
            {
                noteList.Add(new Note(Note.F, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.F, 4));
                noteList.Add(new Note(Note.A, 4));
                noteList.Add(new Note(Note.C, 5));
                noteList.Add(new Note(Note.F, 5));
            }
            else if (noteType == Note.A)
            {
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.G, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.G, 4));
            }
            else if (noteType == Note.D || noteType == Note.DSharp)
            {
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.G, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.E, 4));
            }
            else if (noteType == Note.B || noteType == Note.ASharp)
            {
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.G, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.G, 4));
            }
            else if (noteType == Note.GSharp)
            {
                noteList.Add(new Note(Note.GSharp, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.DSharp, 4));
                noteList.Add(new Note(Note.GSharp, 4));
                noteList.Add(new Note(Note.C, 5));
                noteList.Add(new Note(Note.DSharp, 5));
            }
            else
            {
                noteList.Add(new Note(Note.C, 3));
                noteList.Add(new Note(Note.G, 3));
                noteList.Add(new Note(Note.C, 4));
                noteList.Add(new Note(Note.E, 4));
                noteList.Add(new Note(Note.G, 4));
                noteList.Add(new Note(Note.C, 5));
            }
            break;
            case maj13:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.E, 4));
            noteList.Add(new Note(Note.A, 4));
            noteList.Add(new Note(Note.D, 5));
            noteList.Add(new Note(Note.FSharp, 5));
            break;
            case maj7:
            noteList.Add(new Note(Note.C, 3));
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.E, 4));
            noteList.Add(new Note(Note.G, 4));
            break;
            case maj9:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.FSharp, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.DSharp, 5));
            noteList.Add(new Note(Note.GSharp, 5));
            break;
            case mma:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.DSharp, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.E, 5));
            break;
            case nine:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.GSharp, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.FSharp, 5));
            break;
            case nineSusFour:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.C, 4));
            noteList.Add(new Note(Note.F, 4));
            noteList.Add(new Note(Note.A, 4));
            noteList.Add(new Note(Note.C, 5));
            noteList.Add(new Note(Note.F, 5));
            break;
            case seven:
            if (noteType == Note.A)
            {
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.D, 4));
                noteList.Add(new Note(Note.GSharp, 4));
                noteList.Add(new Note(Note.B, 4));
            }
            else
            {
                noteList.Add(new Note(Note.E, 3));
                noteList.Add(new Note(Note.B, 3));
                noteList.Add(new Note(Note.D, 4));
                noteList.Add(new Note(Note.GSharp, 4));
                noteList.Add(new Note(Note.B, 4));
                noteList.Add(new Note(Note.E, 5));
            }
            break;
            case sevenBFive:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.CSharp, 4));
            noteList.Add(new Note(Note.F, 4));
            noteList.Add(new Note(Note.B, 4));
            noteList.Add(new Note(Note.CSharp, 5));
            noteList.Add(new Note(Note.G, 5));
            break;
            case sevenBNine:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.GSharp, 4));
            noteList.Add(new Note(Note.D, 5));
            noteList.Add(new Note(Note.F, 5));
            break;
            case sevenSusFour:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.F, 4));
            noteList.Add(new Note(Note.C, 5));
            noteList.Add(new Note(Note.D, 5));
            noteList.Add(new Note(Note.G, 5));
            break;
            case six:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.E, 4));
            noteList.Add(new Note(Note.GSharp, 4));
            noteList.Add(new Note(Note.CSharp, 5));
            noteList.Add(new Note(Note.E, 5));
            break;
            case sixSlashNine:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.E, 4));
            noteList.Add(new Note(Note.A, 4));
            noteList.Add(new Note(Note.D, 5));
            noteList.Add(new Note(Note.G, 5));
            break;
            case sus2:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.A, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.A, 4));
            noteList.Add(new Note(Note.D, 5));
            noteList.Add(new Note(Note.G, 5));
            break;
            case sus4:
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.C, 5));
            noteList.Add(new Note(Note.D, 5));
            noteList.Add(new Note(Note.G, 5));
            break;
            case thirteen:
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.B, 3));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.GSharp, 4));
            noteList.Add(new Note(Note.CSharp, 5));
            noteList.Add(new Note(Note.FSharp, 5));
            break;
            case ff_maj:
            noteList.Add(new Note(Note.C, 1));
            noteList.Add(new Note(Note.D, 1));
            noteList.Add(new Note(Note.E, 1));
            noteList.Add(new Note(Note.G, 1));
            noteList.Add(new Note(Note.C, 2));
            noteList.Add(new Note(Note.D, 2));
            noteList.Add(new Note(Note.E, 2));
            noteList.Add(new Note(Note.G, 2));
            noteList.Add(new Note(Note.C, 3));
            noteList.Add(new Note(Note.D, 3));
            noteList.Add(new Note(Note.E, 3));
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.C, 4));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.E, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.C, 5));
            break;
            case ff_min:
            noteList.Add(new Note(Note.C, 1));
            noteList.Add(new Note(Note.D, 1));
            noteList.Add(new Note(Note.EFlat, 1));
            noteList.Add(new Note(Note.G, 1));
            noteList.Add(new Note(Note.C, 2));
            noteList.Add(new Note(Note.D, 2));
            noteList.Add(new Note(Note.EFlat, 2));
            noteList.Add(new Note(Note.G, 2));
            noteList.Add(new Note(Note.C, 3));
            noteList.Add(new Note(Note.D, 3));
            noteList.Add(new Note(Note.EFlat, 3));
            noteList.Add(new Note(Note.G, 3));
            noteList.Add(new Note(Note.C, 4));
            noteList.Add(new Note(Note.D, 4));
            noteList.Add(new Note(Note.EFlat, 4));
            noteList.Add(new Note(Note.G, 4));
            noteList.Add(new Note(Note.C, 5));
            break;
            default:
            break;
        }
        setFundamental(noteType);
        name = noteList[0].getName() + " " + chordType.ToString();

        if (instrument.getStringCount() > 6)
        StringExpander.addMoreNotes(this, instrument.getStringCount());

        fixDuplicateNoteToOctave();

        if (instrument.getStringCount() < 6)
        StringExpander.removeSomeNotes(this, instrument.getStringCount());
    }

    /**
    * Create chord from string
    * @param serialized serialized chord
    */
    public Chord(string serialized)
    {
        AppController appController = AppController.getAppController();
        Instrument instrument = appController.getInstrument();

        string[] segmentList = serialized.Split(":");

        name = segmentList[0].Trim();

        chordTypeId = int.Parse(segmentList[1].Trim());

        noteType = int.Parse(segmentList[2].Trim());

        Chord prototypeChord = new Chord(noteType, Chord.getChordTypeFromId(chordTypeId));

        foreach (Note note in prototypeChord)
        {
            noteList.Add(note);
        }

        if (instrument.getStringCount() > 6)
        StringExpander.addMoreNotes(this, instrument.getStringCount());
        else if (instrument.getStringCount() < 6)
        StringExpander.removeSomeNotes(this, instrument.getStringCount());
    }

    private void fixDuplicateNoteToOctave()
    {
        if (noteList[1].getPitch() == noteList[2].getPitch())
        noteList[1].setPitch(noteList[1].getPitch() - 12);

        if (noteList[0].getPitch() == noteList[1].getPitch())
        noteList[0].setPitch(noteList[0].getPitch() - 12);

        if (noteList[0].getPitch() == noteList[2].getPitch())
        noteList[0].setPitch(noteList[0].getPitch() - 12);

        if (noteList[0].getPitch() == noteList[1].getPitch())
        noteList[0].setPitch(noteList[0].getPitch() - 12);
    }

    /**
    * @param newFundamental will transpose chord to specified fundamental note
    */
    public void setFundamental(int newFundamental)
    {
        int minNote = Note.E + 2 * 12;
        int maxNote = Note.DSharp + 3 * 12;

        while (newFundamental > maxNote)
        newFundamental -= 12;
        while (newFundamental < minNote)
        newFundamental += 12;

        int currentFundamental = noteList[0].getPitch();

        int offset = newFundamental - currentFundamental;

        foreach (Note note in noteList)
        {
            if (note != null)
            note.setPitch(note.getPitch() + offset);
        }
    }

    public override string ToString()
    {
        return name;
    }

    public Note get(int index)
    {
        return noteList[index];
    }

    // C# indexer and Count property to match usage from converted code
    public Note this[int index] => noteList[index];
    public int Count => noteList.Count;

    public int Size()
    {
        return noteList.Count;
    }

    public IEnumerator<Note> GetEnumerator()
    {
        return noteList.GetEnumerator();
    }

    public bool ContainsExact(Note note)
    {
        foreach (Note other in noteList)
        {
            if (note.getPitch() == other.getPitch())
            return true;
        }
        return false;
    }

    // Java-style wrappers
    public bool containsExact(Note note) => ContainsExact(note);
    public void addNote(Note note) => AddNote(note);
    public int lastPitch() => LastPitch();
    public string serialize() => Serialize();
    public int getChordTypeId() => GetChordTypeId();
    public int getNoteType() => GetNoteType();
    public int getAveragePitch() => GetAveragePitch();

    public void AddNote(Note note)
    {
        noteList.Add(note);
    }

    public int LastPitch()
    {
        int index = noteList.Count - 1;
        while (index >= 0)
        {
            if (noteList[index] != null)
            return noteList[index].getPitch();
            index--;
        }
        return 0;
    }

    public string Serialize()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append(name.Trim());

        stringBuilder.Append(':');
        stringBuilder.Append(chordTypeId);

        stringBuilder.Append(':');
        stringBuilder.Append(noteType);

        return stringBuilder.ToString();
    }

    public int GetKey()
    {
        return noteList[0].getPitch() % 12;
    }

    public int GetChordTypeId()
    {
        return chordTypeId;
    }

    public int GetNoteType()
    {
        return noteType;
    }

    private static int getChordTypeId(ChordType chordType)
    {
        switch (chordType)
        {
            case maj:
            return 0;
            case maj7:
            return 1;
            case maj9:
            return 2;
            case maj13:
            return 3;
            case m:
            return 4;
            case m6:
            return 5;
            case m7:
            return 6;
            case m9:
            return 7;
            case m11:
            return 8;
            case m13:
            return 9;
            case mma:
            return 10;
            case aug:
            return 11;
            case aug9:
            return 12;
            case add9:
            return 13;
            case sus2:
            return 14;
            case sus4:
            return 15;
            case five:
            return 16;
            case six:
            return 17;
            case sixSlashNine:
            return 18;
            case seven:
            return 19;
            case sevenSusFour:
            return 20;
            case sevenBFive:
            return 21;
            case sevenBNine:
            return 22;
            case nine:
            return 23;
            case nineSusFour:
            return 24;
            case eleven:
            return 25;
            case thirteen:
            return 26;
            case ff_maj:
            return 27;
            case ff_min:
            return 28;
            default:
            return -1;
        }
    }

    public static ChordType getChordTypeFromId(int index)
    {
        switch (index)
        {
            case 0:
            return ChordType.maj;
            case 1:
            return ChordType.maj7;
            case 2:
            return ChordType.maj9;
            case 3:
            return ChordType.maj13;
            case 4:
            return ChordType.m;
            case 5:
            return ChordType.m6;
            case 6:
            return ChordType.m7;
            case 7:
            return ChordType.m9;
            case 8:
            return ChordType.m11;
            case 9:
            return ChordType.m13;
            case 10:
            return ChordType.mma;
            case 11:
            return ChordType.aug;
            case 12:
            return ChordType.aug9;
            case 13:
            return ChordType.add9;
            case 14:
            return ChordType.sus2;
            case 15:
            return ChordType.sus4;
            case 16:
            return ChordType.five;
            case 17:
            return ChordType.six;
            case 18:
            return ChordType.sixSlashNine;
            case 19:
            return ChordType.seven;
            case 20:
            return ChordType.sevenSusFour;
            case 21:
            return ChordType.sevenBFive;
            case 22:
            return ChordType.sevenBNine;
            case 23:
            return ChordType.nine;
            case 24:
            return ChordType.nineSusFour;
            case 25:
            return ChordType.eleven;
            case 26:
            return ChordType.thirteen;
            case 27:
            return ChordType.ff_maj;
            case 28:
            return ChordType.ff_min;
            default:
            return ChordType.maj;
        }
    }

    public void remove(Note note)
    {
        noteList.Remove(note);
    }

    public int GetAveragePitch()
    {
        int pitchSum = 0;

        foreach (Note note in noteList)
        pitchSum += note.getPitch();

        if (noteList.Count == 0)
            return 0;

        return (int)Math.Round((double)pitchSum / (double)noteList.Count);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
