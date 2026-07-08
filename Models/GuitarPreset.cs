package com.virtuophone.models;

import java.util.ArrayList;
import java.util.Iterator;


import android.graphics.Point;

/**
 * @author Guillaume Lacasse
 * Chord preset
 */
public class GuitarPreset implements Iterable<Chord>, StringSerializable {
	private String name = null;
	
	private ArrayList<Chord> chordList = new ArrayList<Chord>();
	
	/**
	 * Create new emptu preset
	 */
	public GuitarPreset() {
		name = "Untitled";
	}

	/**
	 * Create preset from serialized string
	 * @param serialized preset as string
	 */
	public GuitarPreset(String serialized) {
		serialized = serialized.replace('\r', '\n');
		String[] lineList = serialized.split("\n");
		
		name = lineList[0].trim();
		
		for (int i = 1; i < lineList.length; i++) {
			String chordAsString = lineList[i].trim();
			if (chordAsString.length() > 1) {
				Chord chord = new Chord(chordAsString);
				chordList.add(chord);
			}
		}
	}

	/**
	 * @return custom name for preset
	 */
	public String getName() {
		return name;
	}

	/**
	 * @param name custom name for preset
	 */
	public void setName(String name) {
		this.name = name;
	}
	
	public void setChord(int index, Chord chord) {
		while (index >= chordList.size())
			chordList.add(new Chord(Note.E,ChordType.m));
		chordList.set(index, chord);
	}

	@Override
	public Iterator<Chord> iterator() {
		return chordList.iterator();
	}

	public int size() {
		return chordList.size();
	}

	public Note getNoteAt(Point coordinates) {
		Chord chord = chordList.get(coordinates.y);
		Note note = chord.get(coordinates.x);
		return note;
	}
	
	public int getChordFundamentalNoteTypeAt(Point coordinates) {
		Chord chord = chordList.get(coordinates.y);
		Note note = chord.get(0);
		return note.getPitch() % 12;
	}
	
	public String serialize() {
		StringBuilder stringBuilder = new StringBuilder();
		
		stringBuilder.append(name.trim());
		stringBuilder.append('\r');
		
		for (Chord chord : chordList) {
			if (chord != null) {
				stringBuilder.append(chord.serialize().trim());
				stringBuilder.append('\r');
			}
		}
		
		return stringBuilder.toString();
	}

	public void replaceChord(Chord oldChord, Chord newChord) {
		int index = chordList.indexOf(oldChord);
		chordList.set(index, newChord);
	}

	public void removeChord(Chord chord) {
		chordList.remove(chordList.indexOf(chord));
	}

	public void add(Chord chord) {
		chordList.add(chord);
	}

	public void regenerateChords() {
		int index = 0;
		for (Chord chord : chordList) {
			Chord newChord = new Chord(chord.getNoteType(), Chord.getChordTypeFromId(chord.getChordTypeId()));
			chordList.set(index, newChord);
			index++;
		}
	}
}