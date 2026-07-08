package com.virtuophone.models.instruments;

import android.content.Context;

import com.virtuophone.R;
import com.virtuophone.models.Note;

public class JewsHarp extends Instrument {
	public JewsHarp(Context context) {
		super(context);
	}

	@Override
	protected void loadSamples() {
		Note note;
		
		note = new Note(Note.DSharp, 2);
		addSample(note, R.raw.guimb5);
		addSample(note, R.raw.guimb9);
		addSample(note, R.raw.guimb15);
		addSample(note, R.raw.guimb17);
		
		note = new Note(Note.DSharp, 3);
		addSample(note, R.raw.guimb36);
		addSample(note, R.raw.guimb2);
		addSample(note, R.raw.guimb3);
		addSample(note, R.raw.guimb8);
		addSample(note, R.raw.guimb10);
		addSample(note, R.raw.guimb12);
		addSample(note, R.raw.guimb23);
		addSample(note, R.raw.guimb30);
		addSample(note, R.raw.guimb31);
		addSample(note, R.raw.guimb34);
		addSample(note, R.raw.guimb35);
		
		note = new Note(Note.DSharp, 4);
		addSample(note, R.raw.guimb7);
		addSample(note, R.raw.guimb6);
		addSample(note, R.raw.guimb25);
		addSample(note, R.raw.guimb32);
		
		note = new Note(Note.DSharp, 5);
		addSample(note, R.raw.guimb14);
		addSample(note, R.raw.guimb1);
		addSample(note, R.raw.guimb11);
		addSample(note, R.raw.guimb20);
		addSample(note, R.raw.guimb26);
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
		return 6;
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
