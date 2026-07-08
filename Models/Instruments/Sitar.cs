package com.virtuophone.models.instruments;

import com.virtuophone.models.Drone;
import com.virtuophone.models.Note;

import com.virtuophone.R;

import android.content.Context;


/**
 * @author Guillaume Lacasse
 * sitar
 */
public class Sitar extends Instrument {
	public Sitar(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		addSample(new Note(Note.E, 1), R.raw.sitaree2);
		addSample(new Note(Note.A, 1), R.raw.sitarea2);
		addSample(new Note(Note.D, 2), R.raw.sitared3);
		addSample(new Note(Note.G, 2), R.raw.sitareg3);
		addSample(new Note(Note.B, 2), R.raw.sitareb3);
		addSample(new Note(Note.E, 3), R.raw.sitaree4);
		
		addSample(new Note(Note.A, 3), R.raw.sitarea4,0.5f);
		addSample(new Note(Note.D, 4), R.raw.sitared5,0.4f);
		addSample(new Note(Note.G, 4), R.raw.sitareg5,0.3f);
		addSample(new Note(Note.B, 4), R.raw.sitareb5,0.3f);
		addSample(new Note(Note.CSharp, 5), R.raw.sitarecsharp6,0.25f);
		addSample(new Note(Note.E, 5), R.raw.sitaree6,0.2f);
	}
	
	@Override
	protected void loadDrone(Context context) {
		setDrone(new Drone(Note.CSharp, R.raw.tampuracsharpshort, 0.3f, 1.2f, 1.01f), context);
		//setDrone(new Drone(Note.C, R.raw.sitardroneshortc, 0.3f, 1.035f), context);
	}
	
	@Override
	protected boolean buildIsLazyHarmonicDrone() {
		return true;
	}
	
	@Override
	protected boolean buildIsDroneMinimizePitchShift() {
		return false;
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
	protected boolean buildIsMuteOnChangeFretSameString() {
		return true;
	}

	@Override
	protected boolean buildIsPitchBend() {
		return true;
	}
	
	@Override
	protected boolean buildIsAutoLoop() {
		return false;
	}
	
	@Override
	protected boolean buildIsAutoLoopKeepNoteUntilNewNote() {
		return false;
	}
}