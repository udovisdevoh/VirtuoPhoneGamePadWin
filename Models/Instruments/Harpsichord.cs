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

    protected void loadSamples()
    {
        addSample(new Note(Note.E, 0), R.raw.harpsichordf0);
        addSample(new Note(Note.A, 0), R.raw.harpsichorda0);
        addSample(new Note(Note.D, 1), R.raw.harpsichordd1);
        addSample(new Note(Note.G, 1), R.raw.harpsichordg1);
        addSample(new Note(Note.B, 1), R.raw.harpsichordb1);
        addSample(new Note(Note.E, 2), R.raw.harpsichorde2);
        addSample(new Note(Note.A, 2), R.raw.harpsichorda2);
        addSample(new Note(Note.D, 3), R.raw.harpsichordd3);
        addSample(new Note(Note.G, 3), R.raw.harpsichordg3);
        addSample(new Note(Note.B, 3), R.raw.harpsichordb3);
        addSample(new Note(Note.E, 4), R.raw.harpsichorde4);
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
        //no drone
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
