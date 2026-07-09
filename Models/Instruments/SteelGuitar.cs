using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;
public class SteelGuitar : Instrument
{
    public SteelGuitar()
    {
    }

    protected void LoadSamples()
    {
        AddSample(new Note(Note.E, 2), R.Raw.guitare2);
        AddSample(new Note(Note.A, 2), R.Raw.guitara2);
        AddSample(new Note(Note.D, 3), R.Raw.guitard3);
        AddSample(new Note(Note.G, 3), R.Raw.guitarg3);
        AddSample(new Note(Note.B, 3), R.Raw.guitarb3);
        AddSample(new Note(Note.E, 4), R.Raw.guitare4);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 2).GetPitch();
    }

    protected int BuildStringCount()
    {
        return 6;
    }

    protected bool BuildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected bool BuildIsAutoLoop()
    {
        return false;
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
