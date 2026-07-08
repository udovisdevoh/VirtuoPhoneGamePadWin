package com.virtuophone.models;

/**
 * Represents a music note
 * @author Guillaume Lacasse
 */
public class Note {
	private int pitch;
	
	public static final int C = 0;
	
	public static final int CSharp = 1;
	
	public static final int DFlat = 1;
	
	public static final int D = 2;
	
	public static final int DSharp = 3;
	
	public static final int EFlat = 3;
	
	public static final int E = 4;
	
	public static final int F = 5;
	
	public static final int FSharp = 6;
	
	public static final int GFlat = 6;
	
	public static final int G = 7;
	
	public static final int GSharp = 8;
	
	public static final int AFlat = 8;
	
	public static final int A = 9;
	
	public static final int ASharp = 10;
	
	public static final int BFlat = 10;
	
	public static final int B = 11;
	
	/**
	 * @param noteType Use constant defined in Note class
	 * @param octave octave number
	 */
	public Note(int noteType, int octave) {
		noteType = noteType % 12;
		pitch = noteType + octave * 12;
	}
	
	public String getName() {
		int noteType = pitch % 12;
		switch (noteType){
			case 0:
				return "C";
			case 1:
				return "C#";
			case 2:
				return "D";
			case 3:
				return "D#";
			case 4:
				return "E";
			case 5:
				return "F";
			case 6:
				return "F#";
			case 7:
				return "G";
			case 8:
				return "G#";
			case 9:
				return "A";
			case 10:
				return "A#";
			case 11:
				return "B";
			default:
				return "";
		}
	}

	/**
	 * @param pitch absolute pitch (semitones)
	 */
	public Note(int pitch) {
		this.pitch = pitch;
	}

	/**
	 * @return Pitch (in semitones)
	 */
	public int getPitch() {
		return pitch;
	}

	public void setPitch(int pitch) {
		this.pitch = pitch;
	}
}