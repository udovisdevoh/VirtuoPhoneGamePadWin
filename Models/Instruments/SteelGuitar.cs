using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;
public class SteelGuitar : Instrument
{
    public SteelGuitar(Context context) : base(context)
    {
    }

    protected void LoadSamples()
    {
        addSample(new Note(Note.E, 2), R.Raw.guitare2);
        addSample(new Note(Note.A, 2), R.Raw.guitara2);
        addSample(new Note(Note.D, 3), R.Raw.guitard3);
        addSample(new Note(Note.G, 3), R.Raw.guitarg3);
        addSample(new Note(Note.B, 3), R.Raw.guitarb3);
        addSample(new Note(Note.E, 4), R.Raw.guitare4);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 2).getPitch();
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
