using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;






public class JewsHarp : Instrument
{
    public JewsHarp(Context context)
    {
        base(context);
    }

    protected void loadSamples()
    {
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

    protected void loadDrone(Context context)
    {
        //setDrone(new Drone(Note.E, R.raw.violine2, 0.3f), context);
    }

    protected int buildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
    }

    protected int buildStringCount()
    {
        return 6;
    }

    protected bool buildIsMuteOnChangeFretSameString()
    {
        return false;
    }

    protected bool buildIsAutoLoop()
    {
        return false;
    }

    protected bool buildIsPitchBend()
    {
        return false;
    }

    protected bool buildIsLazyHarmonicDrone()
    {
        return false;
    }

    protected bool buildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected bool buildIsAutoLoopKeepNoteUntilNewNote()
    {
        return false;
    }
}
