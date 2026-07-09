using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Bagpipes : Instrument
{
    public Bagpipes(Context context) : base(context)
    {
    }

    protected void LoadSamples()
    {
        addSample(new Note(Note.E, 2), R.Raw.bagpipese0);
        addSample(new Note(Note.A, 2), R.Raw.bagpipesa0);
        addSample(new Note(Note.D, 3), R.Raw.bagpipesd1);
        addSample(new Note(Note.G, 3), R.Raw.bagpipesg1);
        addSample(new Note(Note.B, 3), R.Raw.bagpipesb1);
        addSample(new Note(Note.E, 4), R.Raw.bagpipese2);
        addSample(new Note(Note.A, 4), R.Raw.bagpipesa2);
    }

    protected void LoadDrone(Context context)
    {
        setDrone(new Drone(Note.A, R.Raw.bagpipesdroneloopa, 1f, 1.2f, 0f), context);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
    }

    protected int BuildStringCount()
    {
        return 6;
    }

    protected bool BuildIsLazyHarmonicDrone()
    {
        return false;
    }

    protected bool BuildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected bool BuildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected bool BuildIsAutoLoop()
    {
        return true;
    }

    protected bool BuildIsPitchBend()
    {
        return false;
    }

    protected bool BuildIsAutoLoopKeepNoteUntilNewNote()
    {
        return true;
    }
}
