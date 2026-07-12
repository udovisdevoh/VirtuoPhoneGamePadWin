using System;
using System.Threading;
using System.Windows.Forms;
using VirtuoPhone;
using VirtuoPhone.Config;
using VirtuoPhone.Engine;
using VirtuoPhone.Input;
using VirtuoPhone.UI;

internal static class Program
{
    [STAThread]   // WinForms requires the UI thread to be single-threaded-apartment
    private static void Main()
    {
        if (Environment.GetEnvironmentVariable("VP_HEADLESS") == "1")
        {
            RunHeadless();
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    // Headless mode (VP_HEADLESS=1): run the engine with console logging, no window — handy for quick tests.
    private static void RunHeadless()
    {
        AppConfig config = ConfigStore.Load();
        AudioBackend.Settings = config.Audio;
        int deviceId = WinmmControllerInput.FindFirstConnected();
        var input = new MappedControllerInput(deviceId, config.Controls);
        Console.WriteLine($"[play] headless — input: {input.Name}");

        var engine = new PlayEngine(input, config.ActivePresetOrDefault(), config.Instrument);
        engine.Status += Console.WriteLine;
        engine.Start();
        Console.WriteLine("[play] running — buttons=notes, stick=chord, Select=instrument, Start=modulate. Ctrl+C to quit.");
        Thread.Sleep(Timeout.Infinite);
    }
}
