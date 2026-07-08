package com.virtuophone.models.instruments;

import com.virtuophone.models.Note;

import com.virtuophone.R;

import android.content.Context;


/**
 * @author Guillaume Lacasse
 * steel guitar
 */
public class Harpsichord extends Instrument {
	public Harpsichord(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		addSample(new Note(Note.E, 0), R.raw.harpsichordf0);
		addSample(new Note(Note.A, 0), R.raw.harpsichorda0);
		addSample(new Note(Note.D, 1), R.raw.harpsichordd1);
		addSample(new Note(Note.G, 1), R.raw.harpsichordg1);
		addSample(new Note(Note.B, 1), R.raw.harpsichordb1);
		addSample(new Note(Note.E, 2), R.raw.harpsichorde2);
		addSample(new Note(Note.A, 2), R.raw.harpsichorda2);
		addSample(new Note(Note.D, 3), R.raw.harpsichordd3);
		addSample(new Note(Note.G, 3), R.raw.harpsichordg3);
		addSample(new Note(Note.B, 3), R.raw.harpsichordb3);
		addSample(new Note(Note.E, 4), R.raw.harpsichorde4);
	}

	@Override
	protected int buildMinPitchToPlay() {
		return new Note(Note.CSharp, 0).getPitch();
	}

	@Override
	protected int buildStringCount() {
		return 8;
	}

	@Override
	protected boolean buildIsMuteOnChangeFretSameString() {
		return false;
	}

	@Override
	protected boolean buildIsAutoLoop() {
		return false;
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