using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Synth : Instrument
{
    public Synth()
    {
    }

    protected override void LoadSamples()
    {
        AddSample(new Note(Note.E, 0), R.Raw.synthe0);
        AddSample(new Note(Note.A, 0), R.Raw.syntha0);
        AddSample(new Note(Note.D, 1), R.Raw.synthd1);
        AddSample(new Note(Note.G, 1), R.Raw.synthg1);
        AddSample(new Note(Note.B, 1), R.Raw.synthb1);
        AddSample(new Note(Note.E, 2), R.Raw.synthe2);
        AddSample(new Note(Note.A, 2), R.Raw.syntha2);
        AddSample(new Note(Note.D, 3), R.Raw.synthd3);
        AddSample(new Note(Note.G, 3), R.Raw.synthg3);
        AddSample(new Note(Note.B, 3), R.Raw.synthb3);
        AddSample(new Note(Note.E, 4), R.Raw.synthe4);
    }

    protected override int BuildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).GetPitch();
    }

    protected override int BuildStringCount()
    {
        return AppController.STRING_COUNT;
    }

    protected override bool BuildIsMuteOnChangeFretSameString()
    {
        return false;
    }

    protected override bool BuildIsAutoLoop()
    {
        return false;
    }

    protected override void LoadDrone()
    {
        setDrone(new Drone(Note.E, R.Raw.violine2, 0.5f, 1.035f, 1.005f));
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
