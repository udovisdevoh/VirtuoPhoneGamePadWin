using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Bagpipes : Instrument
{
    protected override void LoadSamples()
    {
        AddSample(new Note(Note.E, 2), R.Raw.bagpipese0);
        AddSample(new Note(Note.A, 2), R.Raw.bagpipesa0);
        AddSample(new Note(Note.D, 3), R.Raw.bagpipesd1);
        AddSample(new Note(Note.G, 3), R.Raw.bagpipesg1);
        AddSample(new Note(Note.B, 3), R.Raw.bagpipesb1);
        AddSample(new Note(Note.E, 4), R.Raw.bagpipese2);
        AddSample(new Note(Note.A, 4), R.Raw.bagpipesa2);
    }

    protected override void LoadDrone()
    {
        setDrone(new Drone(Note.A, R.Raw.bagpipesdroneloopa, 1f, 1.2f, 0f));
    }

    protected override int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).GetPitch();
    }

    protected override int BuildStringCount()
    {
        return 8;
    }

    protected override bool BuildIsLazyHarmonicDrone()
    {
        return false;
    }

    protected override bool BuildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected override bool BuildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected override bool BuildIsAutoLoop()
    {
        return true;
    }

    protected override bool BuildIsPitchBend()
    {
        return false;
    }

    protected override bool BuildIsAutoLoopKeepNoteUntilNewNote()
    {
        return true;
    }
}
