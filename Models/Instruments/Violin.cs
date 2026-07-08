package com.virtuophone.models.instruments;

import com.virtuophone.models.Note;

import com.virtuophone.R;

import android.content.Context;


/**
 * @author Guillaume Lacasse
 * violin
 */
public class Violin extends Instrument {
	public Violin(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		addSample(new Note(Note.E, 0), R.raw.violine0);
		addSample(new Note(Note.A, 0), R.raw.violina0);
		addSample(new Note(Note.D, 1), R.raw.violind1);
		addSample(new Note(Note.G, 1), R.raw.violing1);
		addSample(new Note(Note.B, 1), R.raw.violinb1);
		addSample(new Note(Note.E, 2), R.raw.violine2);
		addSample(new Note(Note.A, 2), R.raw.violina2);
		addSample(new Note(Note.D, 3), R.raw.violind3);
		addSample(new Note(Note.G, 3), R.raw.violing3);
		addSample(new Note(Note.B, 3), R.raw.violinb3);
		addSample(new Note(Note.E, 4), R.raw.violine4);
	}

	@Override
	protected int buildMinPitchToPlay() {
		return new Note(Note.CSharp, 0).getPitch();
	}

	@Override
	protected int buildStringCount() {
		return 4;
	}

	@Override
	protected boolean buildIsMuteOnChangeFretSameString() {
		return true;
	}

	@Override
	protected boolean buildIsAutoLoop() {
		return true;
	}
	
	@Override
	protected void loadDrone(Context context) {
		//no drone
	}
	
	@Override
	protected boolean buildIsPitchBend() {
		return false;
	}
	
	@Override
	protected boolean buildIsLazyHarmonicDrone() {
		return false;
	}
	
	@Override
	protected boolean buildIsDroneMinimizePitchShift() {
		return false;
	}
	
	@Override
	protected boolean buildIsAutoLoopKeepNoteUntilNewNote() {
		return false;
	}
}