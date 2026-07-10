using VirtuoPhone;
using VirtuoPhone.Models;

Console.WriteLine("VirtuoPhone — audio smoke test (SteelGuitar via NAudio/WASAPI)");

try
{
    // Builds the instrument -> opens WASAPI and decodes the guitar samples (see [audio] line).
    Instrument instrument = AppController.GetAppController().GetInstrument();
    Console.WriteLine($"[test] instrument ready: strings={instrument.GetStringCount()}, autoLoop={instrument.IsAutoLoop()}");

    // E major arpeggio (E2 B2 E3 G#3 B3 E4), one note per string, ~350 ms apart.
    (int type, int octave)[] notes =
    {
        (Note.E, 2), (Note.B, 2), (Note.E, 3), (Note.GSharp, 3), (Note.B, 3), (Note.E, 4),
    };

    var sw = System.Diagnostics.Stopwatch.StartNew();
    for (int i = 0; i < notes.Length; i++)
    {
        int pitch = new Note(notes[i].type, notes[i].octave).GetPitch();
        int stream = instrument.Play(pitch, i % instrument.GetStringCount(), 0f);
        Console.WriteLine($"[test] +{sw.ElapsedMilliseconds,4} ms  note {i} pitch={pitch} -> stream {stream}");
        Thread.Sleep(350);
    }

    // Worst case for clipping: hit all six notes at once.
    Console.WriteLine("[test] full 6-note strum (max polyphony)…");
    for (int i = 0; i < notes.Length; i++)
        instrument.Play(new Note(notes[i].type, notes[i].octave).GetPitch(), i, 0f);

    Thread.Sleep(2500);   // let the chord ring
    Console.WriteLine("[test] done ringing. Press Enter to exit.");
    Console.ReadLine();
    instrument.Release();
    Console.WriteLine("[test] released. Bye.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[test] FAILED: {ex}");
    Environment.ExitCode = 1;
}
