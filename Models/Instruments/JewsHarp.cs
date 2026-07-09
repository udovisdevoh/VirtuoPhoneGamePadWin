using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;






public class JewsHarp : Instrument
{
    public JewsHarp(Context context) : base(context)
    {
    }

    protected void LoadSamples()
    {
        Note note;

        note = new Note(Note.DSharp, 2);
        addSample(note, R.Raw.guimb5);
        addSample(note, R.Raw.guimb9);
        addSample(note, R.Raw.guimb15);
        addSample(note, R.Raw.guimb17);

        note = new Note(Note.DSharp, 3);
        addSample(note, R.Raw.guimb36);
        addSample(note, R.Raw.guimb2);
        addSample(note, R.Raw.guimb3);
        addSample(note, R.Raw.guimb8);
        addSample(note, R.Raw.guimb10);
        addSample(note, R.Raw.guimb12);
        addSample(note, R.Raw.guimb23);
        addSample(note, R.Raw.guimb30);
        addSample(note, R.Raw.guimb31);
        addSample(note, R.Raw.guimb34);
        addSample(note, R.Raw.guimb35);

        note = new Note(Note.DSharp, 4);
        addSample(note, R.Raw.guimb7);
        addSample(note, R.Raw.guimb6);
        addSample(note, R.Raw.guimb25);
        addSample(note, R.Raw.guimb32);

        note = new Note(Note.DSharp, 5);
        addSample(note, R.Raw.guimb14);
        addSample(note, R.Raw.guimb1);
        addSample(note, R.Raw.guimb11);
        addSample(note, R.Raw.guimb20);
        addSample(note, R.Raw.guimb26);
    }

    protected void LoadDrone(Context context)
    {
        //setDrone(new Drone(Note.E, R.raw.violine2, 0.3f), context);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
    }

    protected int BuildStringCount()
    {
        return 6;
    }

    protected bool BuildIsMuteOnChangeFretSameString()
    {
        return false;
    }

    protected bool BuildIsAutoLoop()
    {
        return false;
    }

    protected bool BuildIsPitchBend()
    {
        return false;
    }

    protected bool BuildIsLazyHarmonicDrone()
    {
        return false;
    }

    protected bool BuildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected bool BuildIsAutoLoopKeepNoteUntilNewNote()
    {
        return false;
    }
}
