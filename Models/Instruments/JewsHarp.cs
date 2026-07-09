using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class JewsHarp : Instrument
{
    public JewsHarp()
    {
    }

    protected override void LoadSamples()
    {
        Note note;

        note = new Note(Note.DSharp, 2);
        AddSample(note, R.Raw.guimb5);
        AddSample(note, R.Raw.guimb9);
        AddSample(note, R.Raw.guimb15);
        AddSample(note, R.Raw.guimb17);

        note = new Note(Note.DSharp, 3);
        AddSample(note, R.Raw.guimb36);
        AddSample(note, R.Raw.guimb2);
        AddSample(note, R.Raw.guimb3);
        AddSample(note, R.Raw.guimb8);
        AddSample(note, R.Raw.guimb10);
        AddSample(note, R.Raw.guimb12);
        AddSample(note, R.Raw.guimb23);
        AddSample(note, R.Raw.guimb30);
        AddSample(note, R.Raw.guimb31);
        AddSample(note, R.Raw.guimb34);
        AddSample(note, R.Raw.guimb35);

        note = new Note(Note.DSharp, 4);
        AddSample(note, R.Raw.guimb7);
        AddSample(note, R.Raw.guimb6);
        AddSample(note, R.Raw.guimb25);
        AddSample(note, R.Raw.guimb32);

        note = new Note(Note.DSharp, 5);
        AddSample(note, R.Raw.guimb14);
        AddSample(note, R.Raw.guimb1);
        AddSample(note, R.Raw.guimb11);
        AddSample(note, R.Raw.guimb20);
        AddSample(note, R.Raw.guimb26);
    }

    protected override void LoadDrone()
    {
        //setDrone(new Drone(Note.E, R.raw.violine2, 0.3f), context);
    }

    protected override int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).GetPitch();
    }

    protected override int BuildStringCount()
    {
        return 6;
    }

    protected override bool BuildIsMuteOnChangeFretSameString()
    {
        return false;
    }

    protected override bool BuildIsAutoLoop()
    {
        return false;
    }

    protected override bool BuildIsPitchBend()
    {
        return false;
    }

    protected override bool BuildIsLazyHarmonicDrone()
    {
        return false;
    }

    protected override bool BuildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected override bool BuildIsAutoLoopKeepNoteUntilNewNote()
    {
        return false;
    }
}
