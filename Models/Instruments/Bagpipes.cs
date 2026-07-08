package com.virtuophone.models.instruments;

import com.virtuophone.models.Drone;
import com.virtuophone.models.Note;

import com.virtuophone.R;

import android.content.Context;


/**
 * @author Guillaume Lacasse
 * bagpipes
 */
public class Bagpipes extends Instrument {
	public Bagpipes(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		addSample(new Note(Note.E, 2), R.raw.bagpipese0);
		addSample(new Note(Note.A, 2), R.raw.bagpipesa0);
		addSample(new Note(Note.D, 3), R.raw.bagpipesd1);
		addSample(new Note(Note.G, 3), R.raw.bagpipesg1);
		addSample(new Note(Note.B, 3), R.raw.bagpipesb1);
		addSample(new Note(Note.E, 4), R.raw.bagpipese2);
		addSample(new Note(Note.A, 4), R.raw.bagpipesa2);
	}
	
	@Override
	protected void loadDrone(Context context) {
		setDrone(new Drone(Note.A, R.raw.bagpipesdroneloopa, 1f, 1.2f, 0f), context);
	}

	@Override
	protected int buildMinPitchToPlay() {
		return new Note(Note.CSharp, 0).getPitch();
	}

	@Override
	protected int buildStringCount() {
		return 6;
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
	protected boolean buildIsMuteOnChangeFretSameString() {
		return true;
	}

	@Override
	protected boolean buildIsAutoLoop() {
		return true;
	}
	
	@Override
	protected boolean buildIsPitchBend() {
		return false;
	}

	@Override
	protected boolean buildIsAutoLoopKeepNoteUntilNewNote() {
		return true;
	}
}