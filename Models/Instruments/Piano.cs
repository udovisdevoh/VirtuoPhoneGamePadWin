package com.virtuophone.models.instruments;

import com.virtuophone.models.Note;

import com.virtuophone.R;

import android.content.Context;


/**
 * @author Guillaume Lacasse
 * piano
 */
public class Piano extends Instrument {
	public Piano(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		addSample(new Note(Note.E, 0), R.raw.pianoe0);
		addSample(new Note(Note.A, 0), R.raw.pianoa0);
		addSample(new Note(Note.D, 1), R.raw.pianod1);
		addSample(new Note(Note.G, 1), R.raw.pianog1);
		addSample(new Note(Note.B, 1), R.raw.pianob1);
		addSample(new Note(Note.E, 2), R.raw.pianoe2);
		addSample(new Note(Note.A, 2), R.raw.pianoa2);
		addSample(new Note(Note.D, 3), R.raw.pianod3);
		addSample(new Note(Note.G, 3), R.raw.pianog3);
		addSample(new Note(Note.B, 3), R.raw.pianob3);
		addSample(new Note(Note.E, 4), R.raw.pianoe4);
	}
	
	@Override
	protected void loadDrone(Context context) {
		//setDrone(new Drone(Note.E, R.raw.violine2, 0.3f), context);
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