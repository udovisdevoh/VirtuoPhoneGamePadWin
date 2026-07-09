using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;
public class Sitar : Instrument
{
    public Sitar(Context context) : base(context)
    {
    }

    protected void LoadSamples()
    {
        addSample(new Note(Note.E, 1), R.Raw.sitaree2);
        addSample(new Note(Note.A, 1), R.Raw.sitarea2);
        addSample(new Note(Note.D, 2), R.Raw.sitared3);
        addSample(new Note(Note.G, 2), R.Raw.sitareg3);
        addSample(new Note(Note.B, 2), R.Raw.sitareb3);
        addSample(new Note(Note.E, 3), R.Raw.sitaree4);

        addSample(new Note(Note.A, 3), R.Raw.sitarea4, 0.5f);
        addSample(new Note(Note.D, 4), R.Raw.sitared5, 0.4f);
        addSample(new Note(Note.G, 4), R.Raw.sitareg5, 0.3f);
        addSample(new Note(Note.B, 4), R.Raw.sitareb5, 0.3f);
        addSample(new Note(Note.CSharp, 5), R.Raw.sitarecsharp6, 0.25f);
        addSample(new Note(Note.E, 5), R.Raw.sitaree6, 0.2f);
    }

    protected void LoadDrone(Context context)
    {
        setDrone(new Drone(Note.CSharp, R.Raw.tampuracsharpshort, 0.3f, 1.2f, 1.01f), context);
        //setDrone(new Drone(Note.C, R.raw.sitardroneshortc, 0.3f, 1.035f), context);
    }

    protected bool BuildIsLazyHarmonicDrone()
    {
        return true;
    }

    protected bool BuildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
    }

    protected int BuildStringCount()
    {
        return 6;
    }

    protected bool BuildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected bool BuildIsPitchBend()
    {
        return true;
    }

    protected bool BuildIsAutoLoop()
    {
        return false;
    }

    protected bool BuildIsAutoLoopKeepNoteUntilNewNote()
    {
        return false;
    }
}
