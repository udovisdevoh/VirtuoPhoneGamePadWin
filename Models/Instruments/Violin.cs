using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Violin : Instrument
{
    public Violin(Context context) : base(context)
    {
    }

    protected void LoadSamples()
    {
        addSample(new Note(Note.E, 0), R.Raw.violine0);
        addSample(new Note(Note.A, 0), R.Raw.violina0);
        addSample(new Note(Note.D, 1), R.Raw.violind1);
        addSample(new Note(Note.G, 1), R.Raw.violing1);
        addSample(new Note(Note.B, 1), R.Raw.violinb1);
        addSample(new Note(Note.E, 2), R.Raw.violine2);
        addSample(new Note(Note.A, 2), R.Raw.violina2);
        addSample(new Note(Note.D, 3), R.Raw.violind3);
        addSample(new Note(Note.G, 3), R.Raw.violing3);
        addSample(new Note(Note.B, 3), R.Raw.violinb3);
        addSample(new Note(Note.E, 4), R.Raw.violine4);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
    }

    protected int BuildStringCount()
    {
        return 4;
    }

    protected bool BuildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected bool BuildIsAutoLoop()
    {
        return true;
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
