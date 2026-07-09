using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models.Instruments;

public class Violin : Instrument
{
    public Violin(Context context)
    {
        base(context);
    }

    protected void loadSamples()
    {
        addSample(new Note(Note.E, 0), R.raw.violine0);
        addSample(new Note(Note.A, 0), R.raw.violina0);
        addSample(new Note(Note.D, 1), R.raw.violind1);
        addSample(new Note(Note.G, 1), R.raw.violing1);
        addSample(new Note(Note.B, 1), R.raw.violinb1);
        addSample(new Note(Note.E, 2), R.raw.violine2);
        addSample(new Note(Note.A, 2), R.raw.violina2);
        addSample(new Note(Note.D, 3), R.raw.violind3);
        addSample(new Note(Note.G, 3), R.raw.violing3);
        addSample(new Note(Note.B, 3), R.raw.violinb3);
        addSample(new Note(Note.E, 4), R.raw.violine4);
    }

    protected int buildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
    }

    protected int buildStringCount()
    {
        return 4;
    }

    protected bool buildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected bool buildIsAutoLoop()
    {
        return true;
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
