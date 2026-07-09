using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;
public class Harp : Instrument
{
    protected void LoadSamples()
    {
        AddSample(new Note(Note.E, 2), R.Raw.harpe0);
        AddSample(new Note(Note.A, 2), R.Raw.harpa0);
        AddSample(new Note(Note.D, 3), R.Raw.harpd1);
        AddSample(new Note(Note.G, 3), R.Raw.harpg1);
        AddSample(new Note(Note.B, 3), R.Raw.harpb1);
        AddSample(new Note(Note.E, 4), R.Raw.harpe2);
        AddSample(new Note(Note.A, 4), R.Raw.harpa2);
        AddSample(new Note(Note.D, 5), R.Raw.harpd3);
        AddSample(new Note(Note.G, 5), R.Raw.harpg3);
        AddSample(new Note(Note.B, 5), R.Raw.harpb3);
        AddSample(new Note(Note.E, 6), R.Raw.harpe4);
    }

    protected void LoadDrone()
    {
        //setDrone(new Drone(Note.E, R.raw.violine2, 0.3f), context);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).GetPitch();
    }

    protected int BuildStringCount()
    {
        return 17;
    }

    protected bool BuildIsMuteOnChangeFretSameString()
    {
        return false;
    }

    protected bool BuildIsAutoLoop()
    {
        return false;
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
