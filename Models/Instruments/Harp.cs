using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;
public class Harp : Instrument
{
    public Harp(Context context) : base(context)
    {
    }

    protected void LoadSamples()
    {
        addSample(new Note(Note.E, 2), R.Raw.harpe0);
        addSample(new Note(Note.A, 2), R.Raw.harpa0);
        addSample(new Note(Note.D, 3), R.Raw.harpd1);
        addSample(new Note(Note.G, 3), R.Raw.harpg1);
        addSample(new Note(Note.B, 3), R.Raw.harpb1);
        addSample(new Note(Note.E, 4), R.Raw.harpe2);
        addSample(new Note(Note.A, 4), R.Raw.harpa2);
        addSample(new Note(Note.D, 5), R.Raw.harpd3);
        addSample(new Note(Note.G, 5), R.Raw.harpg3);
        addSample(new Note(Note.B, 5), R.Raw.harpb3);
        addSample(new Note(Note.E, 6), R.Raw.harpe4);

        /*addSample(new Note(Note.E, 0), R.raw.harpe0);
        addSample(new Note(Note.A, 0), R.raw.harpa0);
        addSample(new Note(Note.D, 1), R.raw.harpd1);
        addSample(new Note(Note.G, 1), R.raw.harpg1);
        addSample(new Note(Note.B, 1), R.raw.harpb1);
        addSample(new Note(Note.E, 2), R.raw.harpe2);
        addSample(new Note(Note.A, 2), R.raw.harpa2);
        addSample(new Note(Note.D, 3), R.raw.harpd3);
        addSample(new Note(Note.G, 3), R.raw.harpg3);
        addSample(new Note(Note.B, 3), R.raw.harpb3);
        addSample(new Note(Note.E, 4), R.raw.harpe4);*/
    }

    protected void LoadDrone(Context context)
    {
        //setDrone(new Drone(Note.E, R.raw.violine2, 0.3f), context);
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
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
