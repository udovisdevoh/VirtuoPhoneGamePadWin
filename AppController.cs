using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuoPhone.Models;

namespace VirtuoPhone;

public class AppController
{
    public const int STRING_COUNT = 8;
    private static AppController instance = new AppController();
    public static AppController GetAppController() => instance;

    // Instruments the Select button cycles through (all are 8 note buttons).
    private readonly Func<Instrument>[] instrumentFactories =
    {
        () => new SteelGuitar(),
        () => new Piano(),
        () => new Harpsichord(),
        () => new Harp(),
        () => new Sitar(),
        () => new Synth(),
        () => new Violin(),
        () => new Bagpipes(),
        () => new JewsHarp(),
    };
    private int instrumentIndex;
    private Instrument? instrument;

    // The active instrument, built lazily. Building one opens an audio device and decodes its samples, and
    // Chord construction asks for it repeatedly — so keep exactly one alive.
    public Instrument GetInstrument() => instrument ??= instrumentFactories[instrumentIndex]();

    // Select button: release the current instrument's device and switch to the next.
    public Instrument NextInstrument()
    {
        instrument?.Release();
        instrumentIndex = (instrumentIndex + 1) % instrumentFactories.Length;
        instrument = instrumentFactories[instrumentIndex]();
        return instrument;
    }

    public string InstrumentName => (instrument ?? GetInstrument()).GetType().Name;
}

