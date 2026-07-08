package com.virtuophone.models.instruments;

import com.virtuophone.models.Note;

import com.virtuophone.R;

import android.content.Context;


/**
 * @author Guillaume Lacasse
 * steel guitar
 */
public class SteelGuitar extends Instrument {
	public SteelGuitar(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		addSample(new Note(Note.E, 2), R.raw.guitare2);
		addSample(new Note(Note.A, 2), R.raw.guitara2);
		addSample(new Note(Note.D, 3), R.raw.guitard3);
		addSample(new Note(Note.G, 3), R.raw.guitarg3);
		addSample(new Note(Note.B, 3), R.raw.guitarb3);
		addSample(new Note(Note.E, 4), R.raw.guitare4);
	}

	@Override
	protected int buildMinPitchToPlay() {
		return new Note(Note.CSharp, 2).getPitch();
	}

	@Override
	protected int buildStringCount() {
		return 6;
	}

	@Override
	protected boolean buildIsMuteOnChangeFretSameString() {
		return true;
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