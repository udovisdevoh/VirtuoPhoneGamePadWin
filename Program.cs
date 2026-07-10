using VirtuoPhone;
using VirtuoPhone.Models;

Console.WriteLine("VirtuoPhone — pop test (within max polyphony: distinct strings, no mute, no voice-steal)");

try
{
    Instrument instrument = AppController.GetAppController().GetInstrument();
    int strings = instrument.GetStringCount();
    Console.WriteLine($"[test] strings={strings} (<= polyphony), each note on its own string, ringing to natural end.");

    // E major chord: one note per DISTINCT string => at most `strings` overlapping voices, well within
    // polyphony (stringCount*2). No same-string retrigger (no mute), no stealing. Let it ring out fully.
    (int type, int octave)[] chord =
    {
        (Note.E, 2), (Note.B, 2), (Note.E, 3), (Note.GSharp, 3), (Note.B, 3), (Note.E, 4),
    };

    Console.WriteLine("[test] rolling the chord once, then letting every note ring to its fade-out…");
    for (int i = 0; i < chord.Length && i < strings; i++)
    {
        int pitch = new Note(chord[i].type, chord[i].octave).GetPitch();
        int stream = instrument.Play(pitch, i, 0f);   // string i (distinct) => no mute, no steal
        Console.WriteLine($"[test]   string {i} pitch={pitch} -> stream {stream}");
        Thread.Sleep(50);
    }

    Thread.Sleep(9000);   // ring out to natural end (guitar samples decay over several seconds)
    Console.WriteLine("[test] done. Press Enter to exit.");
    Console.ReadLine();
    instrument.Release();
    Console.WriteLine("[test] released.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[test] FAILED: {ex}");
    Environment.ExitCode = 1;
}
