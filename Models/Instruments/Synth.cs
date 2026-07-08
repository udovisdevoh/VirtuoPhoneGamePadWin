package com.virtuophone.models.instruments;

import com.virtuophone.models.Drone;
import com.virtuophone.models.Note;

import com.virtuophone.R;

import android.content.Context;


/**
 * @author Guillaume Lacasse
 * synth
 */
public class Synth extends Instrument {
	public Synth(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		addSample(new Note(Note.E, 0), R.raw.synthe0);
		addSample(new Note(Note.A, 0), R.raw.syntha0);
		addSample(new Note(Note.D, 1), R.raw.synthd1);
		addSample(new Note(Note.G, 1), R.raw.synthg1);
		addSample(new Note(Note.B, 1), R.raw.synthb1);
		addSample(new Note(Note.E, 2), R.raw.synthe2);
		addSample(new Note(Note.A, 2), R.raw.syntha2);
		addSample(new Note(Note.D, 3), R.raw.synthd3);
		addSample(new Note(Note.G, 3), R.raw.synthg3);
		addSample(new Note(Note.B, 3), R.raw.synthb3);
		addSample(new Note(Note.E, 4), R.raw.synthe4);
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
		setDrone(new Drone(Note.E, R.raw.violine2, 0.5f, 1.035f, 1.005f), context);
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