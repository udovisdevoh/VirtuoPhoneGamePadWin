using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models;

public class Violin : Instrument
{
    public Violin()
    {
    }

    protected override void LoadSamples()
    {
        AddSample(new Note(Note.E, 0), R.Raw.violine0);
        AddSample(new Note(Note.A, 0), R.Raw.violina0);
        AddSample(new Note(Note.D, 1), R.Raw.violind1);
        AddSample(new Note(Note.G, 1), R.Raw.violing1);
        AddSample(new Note(Note.B, 1), R.Raw.violinb1);
        AddSample(new Note(Note.E, 2), R.Raw.violine2);
        AddSample(new Note(Note.A, 2), R.Raw.violina2);
        AddSample(new Note(Note.D, 3), R.Raw.violind3);
        AddSample(new Note(Note.G, 3), R.Raw.violing3);
        AddSample(new Note(Note.B, 3), R.Raw.violinb3);
        AddSample(new Note(Note.E, 4), R.Raw.violine4);
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

    protected override bool BuildIsAutoLoop()
    {
        return true;
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

    // Bowed string: short fades so notes don't click on attack or when released.
    protected override float BuildAttackSeconds()
    {
        return 0.04f;
    }

    protected override float BuildReleaseSeconds()
    {
        return 0.08f;
    }

    // Bowed string: glide (portamento) to the new note on chord changes instead of re-attacking.
    protected override float BuildGlissandoSeconds()
    {
        return 0.12f;
    }
}
