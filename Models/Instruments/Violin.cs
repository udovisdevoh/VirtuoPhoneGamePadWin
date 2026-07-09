using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Violin : Instrument
{
    public Violin()
    {
    }

    protected void LoadSamples()
    {
        AddSample(new Note(Note.E, 0), R.Raw.violine0);
        AddSample(new Note(Note.A, 0), R.Raw.violina0);
        AddSample(new Note(Note.D, 1), R.Raw.violind1);
        AddSample(new Note(Note.G, 1), R.Raw.violing1);
        AddSample(new Note(Note.B, 1), R.Raw.violinb1);
        AddSample(new Note(Note.E, 2), R.Raw.violine2);
        AddSample(new Note(Note.A, 2), R.Raw.violina2);
        AddSample(new Note(Note.D, 3), R.Raw.violind3);
        AddSample(new Note(Note.G, 3), R.Raw.violing3);
        AddSample(new Note(Note.B, 3), R.Raw.violinb3);
        AddSample(new Note(Note.E, 4), R.Raw.violine4);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).GetPitch();
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

    protected void LoadDrone()
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
