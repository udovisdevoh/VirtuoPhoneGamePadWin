using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Harpsichord : Instrument
{
    public Harpsichord(Context context) : base(context)
    {
    }

    protected void LoadSamples()
    {
        addSample(new Note(Note.E, 0), R.Raw.harpsichordf0);
        addSample(new Note(Note.A, 0), R.Raw.harpsichorda0);
        addSample(new Note(Note.D, 1), R.Raw.harpsichordd1);
        addSample(new Note(Note.G, 1), R.Raw.harpsichordg1);
        addSample(new Note(Note.B, 1), R.Raw.harpsichordb1);
        addSample(new Note(Note.E, 2), R.Raw.harpsichorde2);
        addSample(new Note(Note.A, 2), R.Raw.harpsichorda2);
        addSample(new Note(Note.D, 3), R.Raw.harpsichordd3);
        addSample(new Note(Note.G, 3), R.Raw.harpsichordg3);
        addSample(new Note(Note.B, 3), R.Raw.harpsichordb3);
        addSample(new Note(Note.E, 4), R.Raw.harpsichorde4);
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
        //no drone
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
