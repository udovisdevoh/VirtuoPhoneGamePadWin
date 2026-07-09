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

    protected override void LoadSamples()
    {
        AddSample(new Note(Note.E, 2), R.Raw.guitare2);
        AddSample(new Note(Note.A, 2), R.Raw.guitara2);
        AddSample(new Note(Note.D, 3), R.Raw.guitard3);
        AddSample(new Note(Note.G, 3), R.Raw.guitarg3);
        AddSample(new Note(Note.B, 3), R.Raw.guitarb3);
        AddSample(new Note(Note.E, 4), R.Raw.guitare4);
    }

    protected override int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 2).GetPitch();
    }

    protected override int BuildStringCount()
    {
        return 6;
    }

    protected override bool BuildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected override bool BuildIsAutoLoop()
    {
        return false;
    }

    protected override void LoadDrone()
    {
        //no drone
    }

    protected override bool BuildIsPitchBend()
    {
        return false;
    }

    protected override bool BuildIsLazyHarmonicDrone()
    {
        return false;
    }

    protected override bool BuildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected override bool BuildIsAutoLoopKeepNoteUntilNewNote()
    {
        return false;
    }
}
