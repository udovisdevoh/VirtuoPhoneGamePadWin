using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace VirtuoPhone.Models.Instruments;









/**
* @author Guillaume Lacasse
* sitar
*/
public class Sitar : Instrument
{
    public Sitar(Context context)
    {
        base(context);
    }

    protected void loadSamples()
    {
        addSample(new Note(Note.E, 1), R.raw.sitaree2);
        addSample(new Note(Note.A, 1), R.raw.sitarea2);
        addSample(new Note(Note.D, 2), R.raw.sitared3);
        addSample(new Note(Note.G, 2), R.raw.sitareg3);
        addSample(new Note(Note.B, 2), R.raw.sitareb3);
        addSample(new Note(Note.E, 3), R.raw.sitaree4);

        addSample(new Note(Note.A, 3), R.raw.sitarea4, 0.5f);
        addSample(new Note(Note.D, 4), R.raw.sitared5, 0.4f);
        addSample(new Note(Note.G, 4), R.raw.sitareg5, 0.3f);
        addSample(new Note(Note.B, 4), R.raw.sitareb5, 0.3f);
        addSample(new Note(Note.CSharp, 5), R.raw.sitarecsharp6, 0.25f);
        addSample(new Note(Note.E, 5), R.raw.sitaree6, 0.2f);
    }

    protected void loadDrone(Context context)
    {
        setDrone(new Drone(Note.CSharp, R.raw.tampuracsharpshort, 0.3f, 1.2f, 1.01f), context);
        //setDrone(new Drone(Note.C, R.raw.sitardroneshortc, 0.3f, 1.035f), context);
    }

    protected bool buildIsLazyHarmonicDrone()
    {
        return true;
    }

    protected bool buildIsDroneMinimizePitchShift()
    {
        return false;
    }

    protected int buildMinPitchToPlay()
    {
        return new Note(Note.CSharp, 0).getPitch();
    }

    protected int buildStringCount()
    {
        return 6;
    }

    protected bool buildIsMuteOnChangeFretSameString()
    {
        return true;
    }

    protected bool buildIsPitchBend()
    {
        return true;
    }

    protected bool buildIsAutoLoop()
    {
        return false;
    }

    protected bool buildIsAutoLoopKeepNoteUntilNewNote()
    {
        return false;
    }
}
