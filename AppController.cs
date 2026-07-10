using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtuoPhone.Models;

namespace VirtuoPhone;

public class AppController
{
    public const int STRING_COUNT = 6;
    private static AppController instance = new AppController();
    public static AppController GetAppController() => instance;
    public Instrument GetInstrument() => new SteelGuitar(); // todo: make this configurable, do not create it here
}

