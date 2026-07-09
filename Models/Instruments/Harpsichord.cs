using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Harpsichord : Instrument
{
    protected override void LoadSamples()
    {
        AddSample(new Note(Note.E, 0), R.Raw.harpsichordf0);
        AddSample(new Note(Note.A, 0), R.Raw.harpsichorda0);
        AddSample(new Note(Note.D, 1), R.Raw.harpsichordd1);
        AddSample(new Note(Note.G, 1), R.Raw.harpsichordg1);
        AddSample(new Note(Note.B, 1), R.Raw.harpsichordb1);
        AddSample(new Note(Note.E, 2), R.Raw.harpsichorde2);
        AddSample(new Note(Note.A, 2), R.Raw.harpsichorda2);
        AddSample(new Note(Note.D, 3), R.Raw.harpsichordd3);
        AddSample(new Note(Note.G, 3), R.Raw.harpsichordg3);
        AddSample(new Note(Note.B, 3), R.Raw.harpsichordb3);
        AddSample(new Note(Note.E, 4), R.Raw.harpsichorde4);
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
        return false;
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
