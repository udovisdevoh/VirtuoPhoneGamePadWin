using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Piano : Instrument
{
    public Piano()
    {
    }

    protected void BoadSamples()
    {
        AddSample(new Note(Note.E, 0), R.Raw.pianoe0);
        AddSample(new Note(Note.A, 0), R.Raw.pianoa0);
        AddSample(new Note(Note.D, 1), R.Raw.pianod1);
        AddSample(new Note(Note.G, 1), R.Raw.pianog1);
        AddSample(new Note(Note.B, 1), R.Raw.pianob1);
        AddSample(new Note(Note.E, 2), R.Raw.pianoe2);
        AddSample(new Note(Note.A, 2), R.Raw.pianoa2);
        AddSample(new Note(Note.D, 3), R.Raw.pianod3);
        AddSample(new Note(Note.G, 3), R.Raw.pianog3);
        AddSample(new Note(Note.B, 3), R.Raw.pianob3);
        AddSample(new Note(Note.E, 4), R.Raw.pianoe4);
    }

    protected void LoadDrone()
    {
        //setDrone(new Drone(Note.E, R.raw.violine2, 0.3f), context);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).GetPitch();
    }

    protected int BuildStringCount()
    {
        return 8;
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
