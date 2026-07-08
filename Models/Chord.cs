package com.virtuophone.models;

import java.util.ArrayList;
import java.util.Iterator;

import com.virtuophone.controllers.AppController;
import com.virtuophone.models.instruments.Instrument;

/**
 * Represents a guitar chord
 * @author Guillaume Lacasse
 */
public class Chord implements Iterable<Note>, StringSerializable {
	private ArrayList<Note> noteList = new ArrayList<Note>();
	
	private String name;
	
	private int noteType;
	
	private int chordTypeId;
	
	/**
	 * @param noteType Use constants (A,B,C etc) defined in Note class
	 * @param chordType chord type
	 * @param isOpen (true: all strings are used, false: some strings may not be used)
	 */
	public Chord(int noteType, ChordType chordType) {
		AppController appController = AppController.getAppController();
		Instrument instrument = appController.getInstrument();
		
		this.noteType = noteType;
		
		
		chordTypeId = getChordTypeId(chordType);
		noteType = noteType % 12;
		switch (chordType) {
			case add9:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.A, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.A, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.G, 5));
				break;
			case aug:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.DSharp, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.G, 5));
				break;
			case aug9:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.A, 3));
				noteList.add(new Note(Note.F, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.DSharp, 5));
				noteList.add(new Note(Note.A, 5));
				break;
			case eleven:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.A, 4));
				noteList.add(new Note(Note.C, 5));
				noteList.add(new Note(Note.F, 5));
				break;
			case five:
				if (noteType == Note.G) {
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.B, 4));
					noteList.add(new Note(Note.E, 5));
				} else {
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.B, 4));
					noteList.add(new Note(Note.E, 5));
					noteList.add(new Note(Note.B, 5));
				}
				break;
			case m:
				if (noteType == Note.A) {
					noteList.add(new Note(Note.A, 2));
					noteList.add(new Note(Note.A, 2));
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.A, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.E, 4));
				} else if (noteType == Note.D) {
					noteList.add(new Note(Note.D, 3));
					noteList.add(new Note(Note.D, 3));
					noteList.add(new Note(Note.D, 3));
					noteList.add(new Note(Note.A, 3));
					noteList.add(new Note(Note.D, 4));
					noteList.add(new Note(Note.F, 4));
				} else if (noteType == Note.DSharp) {
					noteList.add(new Note(Note.D, 3));
					noteList.add(new Note(Note.D, 3));
					noteList.add(new Note(Note.F, 3));
					noteList.add(new Note(Note.A, 3));
					noteList.add(new Note(Note.D, 4));
					noteList.add(new Note(Note.F, 4));
				} else if (noteType == Note.E) {
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.G, 4));
					noteList.add(new Note(Note.B, 4));
					noteList.add(new Note(Note.E, 5));
				} else if (noteType == Note.C) {
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.DSharp, 3));
					noteList.add(new Note(Note.G, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.DSharp, 4));
				} else if (noteType == Note.B || noteType == Note.ASharp || noteType == Note.CSharp) {
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.G, 4));
					noteList.add(new Note(Note.B, 4));
				} else if (noteType == Note.GSharp) {
					noteList.add(new Note(Note.GSharp, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.DSharp, 4));
					noteList.add(new Note(Note.GSharp, 4));
					noteList.add(new Note(Note.DSharp, 5));
					noteList.add(new Note(Note.GSharp, 5));
				} else {
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.G, 4));
					noteList.add(new Note(Note.B, 4));
					noteList.add(new Note(Note.E, 5));
				}
				break;
			case m11:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.A, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.E, 5));
				break;
			case m13:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.CSharp, 5));
				noteList.add(new Note(Note.FSharp, 5));
				break;
			case m6:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.E, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.CSharp, 5));
				noteList.add(new Note(Note.E, 5));
				break;
			case m7:
				if (noteType == Note.D) {
					noteList.add(new Note(Note.D, 2));
					noteList.add(new Note(Note.D, 2));
					noteList.add(new Note(Note.D, 2));
					noteList.add(new Note(Note.A, 2));
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.F, 3));
				} else {
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.G, 4));
					noteList.add(new Note(Note.D, 5));
					noteList.add(new Note(Note.E, 5));
				}
				break;
			case m9:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.FSharp, 5));
				break;
			case maj:
				/*noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.G, 5));*/
				if (noteType == Note.G) {
					noteList.add(new Note(Note.G, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.D, 4));
					noteList.add(new Note(Note.G, 4));
					noteList.add(new Note(Note.B, 4));
					noteList.add(new Note(Note.G, 5));
				} else if (noteType == Note.C || noteType == Note.CSharp) {
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.G, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.E, 4));
				} else if (noteType == Note.F) {
					noteList.add(new Note(Note.F, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.F, 4));
					noteList.add(new Note(Note.A, 4));
					noteList.add(new Note(Note.C, 5));
					noteList.add(new Note(Note.F, 5));
				} else if (noteType == Note.A) {
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.G, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.G, 4));
				} else if (noteType == Note.D || noteType == Note.DSharp) {
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.G, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.E, 4));
				} else if (noteType == Note.B || noteType == Note.ASharp) {
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.G, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.G, 4));
				} else if (noteType == Note.GSharp) {
					noteList.add(new Note(Note.GSharp, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.DSharp, 4));
					noteList.add(new Note(Note.GSharp, 4));
					noteList.add(new Note(Note.C, 5));
					noteList.add(new Note(Note.DSharp, 5));
				} else {
					noteList.add(new Note(Note.C, 3));
					noteList.add(new Note(Note.G, 3));
					noteList.add(new Note(Note.C, 4));
					noteList.add(new Note(Note.E, 4));
					noteList.add(new Note(Note.G, 4));
					noteList.add(new Note(Note.C, 5));
				}
				break;
			case maj13:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.E, 4));
				noteList.add(new Note(Note.A, 4));
				noteList.add(new Note(Note.D, 5));
				noteList.add(new Note(Note.FSharp, 5));
				break;
			case maj7:
				noteList.add(new Note(Note.C, 3));
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.E, 4));
				noteList.add(new Note(Note.G, 4));
				break;
			case maj9:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.FSharp, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.DSharp, 5));
				noteList.add(new Note(Note.GSharp, 5));
				break;
			case mma:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.DSharp, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.E, 5));
				break;
			case nine:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.GSharp, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.FSharp, 5));
				break;
			case nineSusFour:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.C, 4));
				noteList.add(new Note(Note.F, 4));
				noteList.add(new Note(Note.A, 4));
				noteList.add(new Note(Note.C, 5));
				noteList.add(new Note(Note.F, 5));
				break;
			case seven:
				if (noteType == Note.A) {
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.D, 4));
					noteList.add(new Note(Note.GSharp, 4));
					noteList.add(new Note(Note.B, 4));
				} else {
					noteList.add(new Note(Note.E, 3));
					noteList.add(new Note(Note.B, 3));
					noteList.add(new Note(Note.D, 4));
					noteList.add(new Note(Note.GSharp, 4));
					noteList.add(new Note(Note.B, 4));
					noteList.add(new Note(Note.E, 5));
				}
				break;
			case sevenBFive:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.CSharp, 4));
				noteList.add(new Note(Note.F, 4));
				noteList.add(new Note(Note.B, 4));
				noteList.add(new Note(Note.CSharp, 5));
				noteList.add(new Note(Note.G, 5));
				break;
			case sevenBNine:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.GSharp, 4));
				noteList.add(new Note(Note.D, 5));
				noteList.add(new Note(Note.F, 5));
				break;
			case sevenSusFour:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.F, 4));
				noteList.add(new Note(Note.C, 5));
				noteList.add(new Note(Note.D, 5));
				noteList.add(new Note(Note.G, 5));
				break;
			case six:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.E, 4));
				noteList.add(new Note(Note.GSharp, 4));
				noteList.add(new Note(Note.CSharp, 5));
				noteList.add(new Note(Note.E, 5));
				break;
			case sixSlashNine:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.E, 4));
				noteList.add(new Note(Note.A, 4));
				noteList.add(new Note(Note.D, 5));
				noteList.add(new Note(Note.G, 5));
				break;
			case sus2:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.A, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.A, 4));
				noteList.add(new Note(Note.D, 5));
				noteList.add(new Note(Note.G, 5));
				break;
			case sus4:
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.C, 5));
				noteList.add(new Note(Note.D, 5));
				noteList.add(new Note(Note.G, 5));
				break;
			case thirteen:
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.B, 3));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.GSharp, 4));
				noteList.add(new Note(Note.CSharp, 5));
				noteList.add(new Note(Note.FSharp, 5));
				break;
			case ff_maj:
				noteList.add(new Note(Note.C, 1));
				noteList.add(new Note(Note.D, 1));
				noteList.add(new Note(Note.E, 1));
				noteList.add(new Note(Note.G, 1));
				noteList.add(new Note(Note.C, 2));
				noteList.add(new Note(Note.D, 2));
				noteList.add(new Note(Note.E, 2));
				noteList.add(new Note(Note.G, 2));
				noteList.add(new Note(Note.C, 3));
				noteList.add(new Note(Note.D, 3));
				noteList.add(new Note(Note.E, 3));
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.C, 4));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.E, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.C, 5));
				break;
			case ff_min:
				noteList.add(new Note(Note.C, 1));
				noteList.add(new Note(Note.D, 1));
				noteList.add(new Note(Note.EFlat, 1));
				noteList.add(new Note(Note.G, 1));
				noteList.add(new Note(Note.C, 2));
				noteList.add(new Note(Note.D, 2));
				noteList.add(new Note(Note.EFlat, 2));
				noteList.add(new Note(Note.G, 2));
				noteList.add(new Note(Note.C, 3));
				noteList.add(new Note(Note.D, 3));
				noteList.add(new Note(Note.EFlat, 3));
				noteList.add(new Note(Note.G, 3));
				noteList.add(new Note(Note.C, 4));
				noteList.add(new Note(Note.D, 4));
				noteList.add(new Note(Note.EFlat, 4));
				noteList.add(new Note(Note.G, 4));
				noteList.add(new Note(Note.C, 5));
				break;
			default:
				break;
		}
		setFundamental(noteType);
		name = noteList.get(0).getName() + " " + chordType.toString();
		
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
	public Chord(String serialized) {
		AppController appController = AppController.getAppController();
		Instrument instrument = appController.getInstrument();
		
		String[] segmentList = serialized.split(":");
		
		name = segmentList[0].trim();
		
		chordTypeId = Integer.parseInt(segmentList[1].trim());
		
		noteType = Integer.parseInt(segmentList[2].trim());
		
		Chord prototypeChord = new Chord(noteType, Chord.getChordTypeFromId(chordTypeId));
		
		for (Note note : prototypeChord) {
			noteList.add(note);
		}
			
		if (instrument.getStringCount() > 6)
			StringExpander.addMoreNotes(this, instrument.getStringCount());
		else if (instrument.getStringCount() < 6)
			StringExpander.removeSomeNotes(this, instrument.getStringCount());
	}

	private void fixDuplicateNoteToOctave() {
		if (noteList.get(1).getPitch() == noteList.get(2).getPitch())
			noteList.get(1).setPitch(noteList.get(1).getPitch()-12);
		
		if (noteList.get(0).getPitch() == noteList.get(1).getPitch())
			noteList.get(0).setPitch(noteList.get(0).getPitch()-12);
		
		if (noteList.get(0).getPitch() == noteList.get(2).getPitch())
			noteList.get(0).setPitch(noteList.get(0).getPitch()-12);
		
		if (noteList.get(0).getPitch() == noteList.get(1).getPitch())
			noteList.get(0).setPitch(noteList.get(0).getPitch()-12);
	}
	
	/**
	 * @param newFundamental will transpose chord to specified fundamental note
	 */
	public void setFundamental(int newFundamental) {
		int minNote = Note.E + 2 * 12;
		int maxNote = Note.DSharp + 3 * 12;
		
		while (newFundamental > maxNote)
			newFundamental -= 12;
		while (newFundamental < minNote)
			newFundamental += 12;
		
		int currentFundamental = noteList.get(0).getPitch();
		
		int offset = newFundamental - currentFundamental;
		
		for (Note note : noteList) {
			if (note != null)
				note.setPitch(note.getPitch() + offset);
		}
	}
	
	public String toString() {
		return name;
	}

	public Note get(int index) {
		return noteList.get(index);
	}
	
	public int size() {
		return noteList.size();
	}

	@Override
	public Iterator<Note> iterator() {
		return noteList.iterator();
	}

	public boolean containsExact(Note note) {
		for (Note other : noteList) {
			if (note.getPitch() == other.getPitch())
				return true;
		}
		return false;
	}

	public void addNote(Note note) {
		noteList.add(note);		
	}

	public int lastPitch() {
		int index = noteList.size() - 1;
		while (index >= 0) {
			if (noteList.get(index) != null)
				return noteList.get(index).getPitch();
			index--;
		}
		return 0;
	}

	@Override
	public String serialize() {
		StringBuilder stringBuilder = new StringBuilder();
		
		stringBuilder.append(name.trim());
		
		stringBuilder.append(':');
		stringBuilder.append(chordTypeId);
		
		stringBuilder.append(':');
		stringBuilder.append(noteType);
		
		return stringBuilder.toString();
	}

	public int getKey() {
		return noteList.get(0).getPitch() % 12;
	}
	
	public int getChordTypeId() {
		return chordTypeId;
	}
	
	public int getNoteType() {
		return noteType;
	}
	
	private static int getChordTypeId(ChordType chordType) {
		switch (chordType) {
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

	public static ChordType getChordTypeFromId(int index) {
		switch (index) {
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

	public void remove(Note note) {
		noteList.remove(note);
	}

	public int getAveragePitch() {
		int pitchSum = 0;
		
		for (Note note : noteList)
			pitchSum += note.getPitch();
		
		if (noteList.size() == 0)
			return 0;
		
		return (int)Math.round((double)pitchSum / (double)noteList.size());
	}
}