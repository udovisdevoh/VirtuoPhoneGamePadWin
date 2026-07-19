using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuoPhone.Models;

namespace VirtuoPhone;

public class AppController
{
    public const int STRING_COUNT = 21;   // note buttons per chord/scale — 8 fit a fight-stick, the rest are keyboard
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
    // Display names, parallel to instrumentFactories (for the UI / config).
    public static readonly string[] InstrumentNames =
    {
        "Steel Guitar", "Piano", "Harpsichord", "Harp", "Sitar", "Synth", "Violin", "Bagpipes", "Jew's Harp",
    };

    private int instrumentIndex;
    private Instrument? instrument;

    public int InstrumentIndex => instrumentIndex;
    public int InstrumentCount => instrumentFactories.Length;

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

    // Select a specific instrument index (from a saved config or the UI).
    public Instrument SetInstrument(int index)
    {
        int n = instrumentFactories.Length;
        instrument?.Release();
        instrumentIndex = ((index % n) + n) % n;
        instrument = instrumentFactories[instrumentIndex]();
        return instrument;
    }

    // Rebuild the current instrument in place — e.g. after audio settings change (new sound pool / device).
    public Instrument RebuildInstrument()
    {
        instrument?.Release();
        instrument = instrumentFactories[instrumentIndex]();
        return instrument;
    }

    public string InstrumentName => (instrument ?? GetInstrument()).GetType().Name;
}

