using VirtuoPhone;
using VirtuoPhone.Models;
using VirtuoPhone.Input;

Console.WriteLine("VirtuoPhone — play with the Mayflash F500 (buttons = notes, stick = chord).");

int deviceId = WinmmControllerInput.FindFirstConnected();
if (deviceId < 0)
{
    Console.WriteLine("[play] No joystick connected. Plug in the F500 (DInput mode).");
    return;
}

var input = new WinmmControllerInput(deviceId);
Console.WriteLine($"[play] controller: \"{input.Name}\" (slot {deviceId}), {input.NoteButtonCount} note buttons");

Instrument instrument = AppController.GetAppController().GetInstrument();
int strings = instrument.GetStringCount();
Console.WriteLine($"[play] instrument: {strings} strings — the first {strings} note buttons are live.");

// 3×3 chord grid (Chromatic Spiral, CLAUDE.md Example 2), major chords, neutral center = E.
var grid = new Dictionary<Direction, Chord>
{
    [Direction.UpLeft]    = new Chord(Note.C,      ChordType.maj),
    [Direction.Up]        = new Chord(Note.D,      ChordType.maj),
    [Direction.UpRight]   = new Chord(Note.F,      ChordType.maj),
    [Direction.Left]      = new Chord(Note.B,      ChordType.maj),
    [Direction.Neutral]   = new Chord(Note.E,      ChordType.maj),
    [Direction.Right]     = new Chord(Note.FSharp, ChordType.maj),
    [Direction.DownLeft]  = new Chord(Note.ASharp, ChordType.maj),
    [Direction.Down]      = new Chord(Note.A,      ChordType.maj),
    [Direction.DownRight] = new Chord(Note.G,      ChordType.maj),
};

Direction dir = Direction.Neutral;
Chord current = grid[dir];
int lastMask = 0;
int liveButtons = Math.Min(strings, input.NoteButtonCount);
int[] playingPitch = new int[liveButtons];   // last pitch each button is sounding (for held-note swap)
Array.Fill(playingPitch, int.MinValue);

int seconds = int.TryParse(Environment.GetEnvironmentVariable("VP_PLAY_SECONDS"), out int sec) && sec > 0 ? sec : 120;
Console.Write($"[play] neutral chord: {current} — notes:");
for (int i = 0; i < strings && i < input.NoteButtonCount; i++)
    Console.Write($" b{i}={current[i].GetName()}({current[i].GetPitch()})");
Console.WriteLine();
Console.WriteLine($"[play] Play! (joystick = chord, buttons = notes; Home quits; auto-stop {seconds}s)");

var sw = System.Diagnostics.Stopwatch.StartNew();
while (sw.Elapsed.TotalSeconds < seconds)
{
    InputSnapshot snap = input.Poll();
    if (snap.Home) break;

    // Joystick changed the chord while playing: immediately swap the note of every HELD button whose note
    // differs in the new chord — no re-press needed. Buttons whose note is unchanged keep ringing.
    if (snap.Dir != dir)
    {
        dir = snap.Dir;
        current = grid[dir];
        Console.WriteLine($"[play] chord -> {current}  ({dir})");

        int sustained = lastMask & snap.NotesMask;
        for (int i = 0; i < liveButtons; i++)
            if ((sustained & (1 << i)) != 0)
            {
                int newPitch = current[i].GetPitch();
                if (newPitch != playingPitch[i])
                {
                    instrument.Play(current[i], i, 0f);   // same-string mute steals the old voice
                    playingPitch[i] = newPitch;
                }
            }
    }

    // Newly pressed buttons: pluck the current chord's note.
    int rising = snap.NotesMask & ~lastMask;
    for (int i = 0; i < liveButtons; i++)
        if ((rising & (1 << i)) != 0)
        {
            instrument.Play(current[i], i, 0f);
            playingPitch[i] = current[i].GetPitch();
            Console.WriteLine($"[play]   note {i}: {current[i].GetName()}");
        }

    lastMask = snap.NotesMask;
    Thread.Sleep(2);   // ~500 Hz input poll
}

instrument.Release();
Console.WriteLine("[play] bye.");
