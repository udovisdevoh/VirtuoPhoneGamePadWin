using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models.Instruments;








/**
* @author Guillaume Lacasse
* steel guitar
*/
public class SteelGuitar : Instrument
{
    public SteelGuitar(Context context)
    {
        base(context);
    }

    protected void loadSamples()
    {
        addSample(new Note(Note.E, 2), R.raw.guitare2);
        addSample(new Note(Note.A, 2), R.raw.guitara2);
        addSample(new Note(Note.D, 3), R.raw.guitard3);
        addSample(new Note(Note.G, 3), R.raw.guitarg3);
        addSample(new Note(Note.B, 3), R.raw.guitarb3);
        addSample(new Note(Note.E, 4), R.raw.guitare4);
    }

    protected int buildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 2).getPitch();
    }

    protected int buildStringCount()
    {
        return 6;
    }

    protected bool buildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected bool buildIsAutoLoop()
    {
        return false;
    }

    protected void loadDrone(Context context)
    {
        //no drone
    }

    protected bool buildIsPitchBend()
    {
        return false;
    }

    protected bool buildIsLazyHarmonicDrone()
    {
        return false;
    }

    protected bool buildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected bool buildIsAutoLoopKeepNoteUntilNewNote()
    {
        return false;
    }
}
