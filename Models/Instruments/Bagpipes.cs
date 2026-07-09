using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models.Instruments;

public class Bagpipes : Instrument
{
    public Bagpipes(Context context)
    {
        base(context);
    }

    protected void loadSamples()
    {
        addSample(new Note(Note.E, 2), R.raw.bagpipese0);
        addSample(new Note(Note.A, 2), R.raw.bagpipesa0);
        addSample(new Note(Note.D, 3), R.raw.bagpipesd1);
        addSample(new Note(Note.G, 3), R.raw.bagpipesg1);
        addSample(new Note(Note.B, 3), R.raw.bagpipesb1);
        addSample(new Note(Note.E, 4), R.raw.bagpipese2);
        addSample(new Note(Note.A, 4), R.raw.bagpipesa2);
    }

    protected void loadDrone(Context context)
    {
        setDrone(new Drone(Note.A, R.raw.bagpipesdroneloopa, 1f, 1.2f, 0f), context);
    }

    protected int buildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
    }

    protected int buildStringCount()
    {
        return 6;
    }

    protected bool buildIsLazyHarmonicDrone()
    {
        return false;
    }

    protected bool buildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected bool buildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected bool buildIsAutoLoop()
    {
        return true;
    }

    protected bool buildIsPitchBend()
    {
        return false;
    }

    protected bool buildIsAutoLoopKeepNoteUntilNewNote()
    {
        return true;
    }
}
