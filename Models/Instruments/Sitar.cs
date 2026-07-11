using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;
public class Sitar : Instrument
{
    protected override void LoadSamples()
    {
        AddSample(new Note(Note.E, 1), R.Raw.sitaree2);
        AddSample(new Note(Note.A, 1), R.Raw.sitarea2);
        AddSample(new Note(Note.D, 2), R.Raw.sitared3);
        AddSample(new Note(Note.G, 2), R.Raw.sitareg3);
        AddSample(new Note(Note.B, 2), R.Raw.sitareb3);
        AddSample(new Note(Note.E, 3), R.Raw.sitaree4);

        AddSample(new Note(Note.A, 3), R.Raw.sitarea4, 0.5f);
        AddSample(new Note(Note.D, 4), R.Raw.sitared5, 0.4f);
        AddSample(new Note(Note.G, 4), R.Raw.sitareg5, 0.3f);
        AddSample(new Note(Note.B, 4), R.Raw.sitareb5, 0.3f);
        AddSample(new Note(Note.CSharp, 5), R.Raw.sitarecsharp6, 0.25f);
        AddSample(new Note(Note.E, 5), R.Raw.sitaree6, 0.2f);
    }

    protected override void LoadDrone()
    {
        setDrone(new Drone(Note.CSharp, R.Raw.tampuracsharpshort, 0.3f, 1.2f, 1.01f));
        //setDrone(new Drone(Note.C, R.raw.sitardroneshortc, 0.3f, 1.035f), context);
    }

    protected override bool BuildIsLazyHarmonicDrone()
    {
        return true;
    }

    protected override bool BuildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected override int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).GetPitch();
    }

    protected override int BuildStringCount()
    {
        return 8;
    }

    protected override bool BuildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected override bool BuildIsPitchBend()
    {
        return true;
    }

    protected override bool BuildIsAutoLoop()
    {
        return false;
    }

    protected override bool BuildIsAutoLoopKeepNoteUntilNewNote()
    {
        return false;
    }
}
