using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Synth : Instrument
{
    public Synth(Context context) : base(context)
    {
    }

    protected void LoadSamples()
    {
        addSample(new Note(Note.E, 0), R.Raw.synthe0);
        addSample(new Note(Note.A, 0), R.Raw.syntha0);
        addSample(new Note(Note.D, 1), R.Raw.synthd1);
        addSample(new Note(Note.G, 1), R.Raw.synthg1);
        addSample(new Note(Note.B, 1), R.Raw.synthb1);
        addSample(new Note(Note.E, 2), R.Raw.synthe2);
        addSample(new Note(Note.A, 2), R.Raw.syntha2);
        addSample(new Note(Note.D, 3), R.Raw.synthd3);
        addSample(new Note(Note.G, 3), R.Raw.synthg3);
        addSample(new Note(Note.B, 3), R.Raw.synthb3);
        addSample(new Note(Note.E, 4), R.Raw.synthe4);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
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

    protected void LoadDrone(Context context)
    {
        setDrone(new Drone(Note.E, R.Raw.violine2, 0.5f, 1.035f, 1.005f), context);
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
