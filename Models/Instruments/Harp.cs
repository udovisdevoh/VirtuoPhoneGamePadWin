package com.virtuophone.models.instruments;

import android.content.Context;

import com.virtuophone.R;
import com.virtuophone.models.Note;

/**
 * @author Guillaume Lacasse
 * Harp
 */
public class Harp extends Instrument {
	public Harp(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		addSample(new Note(Note.E, 2), R.raw.harpe0);
		addSample(new Note(Note.A, 2), R.raw.harpa0);
		addSample(new Note(Note.D, 3), R.raw.harpd1);
		addSample(new Note(Note.G, 3), R.raw.harpg1);
		addSample(new Note(Note.B, 3), R.raw.harpb1);
		addSample(new Note(Note.E, 4), R.raw.harpe2);
		addSample(new Note(Note.A, 4), R.raw.harpa2);
		addSample(new Note(Note.D, 5), R.raw.harpd3);
		addSample(new Note(Note.G, 5), R.raw.harpg3);
		addSample(new Note(Note.B, 5), R.raw.harpb3);
		addSample(new Note(Note.E, 6), R.raw.harpe4);
		
		/*addSample(new Note(Note.E, 0), R.raw.harpe0);
		addSample(new Note(Note.A, 0), R.raw.harpa0);
		addSample(new Note(Note.D, 1), R.raw.harpd1);
		addSample(new Note(Note.G, 1), R.raw.harpg1);
		addSample(new Note(Note.B, 1), R.raw.harpb1);
		addSample(new Note(Note.E, 2), R.raw.harpe2);
		addSample(new Note(Note.A, 2), R.raw.harpa2);
		addSample(new Note(Note.D, 3), R.raw.harpd3);
		addSample(new Note(Note.G, 3), R.raw.harpg3);
		addSample(new Note(Note.B, 3), R.raw.harpb3);
		addSample(new Note(Note.E, 4), R.raw.harpe4);*/
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
		return 17;
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