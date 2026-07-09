using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Piano : Instrument
{
    public Piano(Context context)
    {
        base(context);
    }

    protected void loadSamples()
    {
        addSample(new Note(Note.E, 0), R.raw.pianoe0);
        addSample(new Note(Note.A, 0), R.raw.pianoa0);
        addSample(new Note(Note.D, 1), R.raw.pianod1);
        addSample(new Note(Note.G, 1), R.raw.pianog1);
        addSample(new Note(Note.B, 1), R.raw.pianob1);
        addSample(new Note(Note.E, 2), R.raw.pianoe2);
        addSample(new Note(Note.A, 2), R.raw.pianoa2);
        addSample(new Note(Note.D, 3), R.raw.pianod3);
        addSample(new Note(Note.G, 3), R.raw.pianog3);
        addSample(new Note(Note.B, 3), R.raw.pianob3);
        addSample(new Note(Note.E, 4), R.raw.pianoe4);
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
        return 8;
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
