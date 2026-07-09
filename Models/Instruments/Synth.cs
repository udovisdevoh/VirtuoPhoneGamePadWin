using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Synth : Instrument
{
    public Synth(Context context)
    {
        base(context);
    }

    protected void loadSamples()
    {
        addSample(new Note(Note.E, 0), R.raw.synthe0);
        addSample(new Note(Note.A, 0), R.raw.syntha0);
        addSample(new Note(Note.D, 1), R.raw.synthd1);
        addSample(new Note(Note.G, 1), R.raw.synthg1);
        addSample(new Note(Note.B, 1), R.raw.synthb1);
        addSample(new Note(Note.E, 2), R.raw.synthe2);
        addSample(new Note(Note.A, 2), R.raw.syntha2);
        addSample(new Note(Note.D, 3), R.raw.synthd3);
        addSample(new Note(Note.G, 3), R.raw.synthg3);
        addSample(new Note(Note.B, 3), R.raw.synthb3);
        addSample(new Note(Note.E, 4), R.raw.synthe4);
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

    protected void loadDrone(Context context)
    {
        setDrone(new Drone(Note.E, R.raw.violine2, 0.5f, 1.035f, 1.005f), context);
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
