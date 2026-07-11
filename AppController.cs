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
    private Instrument? instrument;
    // Cache a single instrument: building one opens an audio device and decodes its samples,
    // and Chord construction asks for it repeatedly. todo: make the instrument configurable.
    public Instrument GetInstrument() => instrument ??= new SteelGuitar();
}

