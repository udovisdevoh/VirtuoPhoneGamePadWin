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
int[] playingPitch = new int[liveButtons];   // last pitch each button is sounding (polyphonic held-note swap)
Array.Fill(playingPitch, int.MinValue);
int[] streamId = new int[liveButtons];        // active stream per button (to stop looping notes on release)
bool lastSelect = false;

// Monophonic instruments (bagpipes chanter) use last-note priority with fall-back to still-held notes.
var heldMono = new List<int>();   // press-order stack of currently-held note buttons
int monoButton = -1;              // the note button currently sounding (-1 = none)
int monoStreamId = 0;

// Drone (bagpipes / sitar): a sustained root that follows the chord. Null for droneless instruments.
Drone? drone = instrument.GetDrone();
drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.GetKey());
int droneTick = 0;

// Interactive play runs until Ctrl+C. Non-interactive/automated runs (stdin redirected) stop on their own
// so they never hang; VP_PLAY_SECONDS forces a fixed duration.
int seconds;
if (int.TryParse(Environment.GetEnvironmentVariable("VP_PLAY_SECONDS"), out int sec) && sec > 0)
    seconds = sec;
else if (Console.IsInputRedirected)
    seconds = 5;
else
    seconds = 0;   // 0 = no time limit
Console.Write($"[play] neutral chord: {current} — notes:");
for (int i = 0; i < strings && i < input.NoteButtonCount; i++)
    Console.Write($" b{i}={current[i].GetName()}({current[i].GetPitch()})");
Console.WriteLine();
Console.WriteLine(seconds > 0
    ? $"[play] Play! (joystick=chord, buttons=notes, Select=instrument; auto-stop {seconds}s)"
    : "[play] Play! (joystick=chord, buttons=notes, Select=change instrument; Ctrl+C to quit)");

var sw = System.Diagnostics.Stopwatch.StartNew();
while (seconds == 0 || sw.Elapsed.TotalSeconds < seconds)
{
    InputSnapshot snap = input.Poll();
    int rising = snap.NotesMask & ~lastMask;
    int falling = lastMask & ~snap.NotesMask;

    // Select cycles the instrument; reset voicing state and re-trigger any held buttons on the new one.
    if (snap.Select && !lastSelect)
    {
        instrument = AppController.GetAppController().NextInstrument();
        Console.WriteLine($"[play] instrument -> {AppController.GetAppController().InstrumentName}");
        drone = instrument.GetDrone();
        drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.GetKey());

        Array.Fill(playingPitch, int.MinValue);
        Array.Fill(streamId, 0);
        heldMono.Clear();
        monoButton = -1;
        monoStreamId = 0;
        for (int i = 0; i < liveButtons; i++)
            if ((snap.NotesMask & (1 << i)) != 0) heldMono.Add(i);

        if (instrument.IsMonophonic())
        {
            monoButton = heldMono.Count > 0 ? heldMono[^1] : -1;
            if (monoButton >= 0) monoStreamId = instrument.Play(current[monoButton], monoButton, 0f);
        }
        else
        {
            for (int i = 0; i < liveButtons; i++)
                if ((snap.NotesMask & (1 << i)) != 0)
                {
                    streamId[i] = instrument.Play(current[i], i, 0f);
                    playingPitch[i] = current[i].GetPitch();
                }
        }
    }
    lastSelect = snap.Select;

    // Joystick changed the chord: retune the drone; held notes are re-voiced below.
    bool chordChanged = snap.Dir != dir;
    if (chordChanged)
    {
        dir = snap.Dir;
        current = grid[dir];
        Console.WriteLine($"[play] chord -> {current}  ({dir})");
        drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.GetKey());
    }

    if (instrument.IsMonophonic())
    {
        // Last-note priority: newest held button sounds; releasing it falls back to the one still held.
        for (int i = 0; i < liveButtons; i++) if ((falling & (1 << i)) != 0) heldMono.Remove(i);
        for (int i = 0; i < liveButtons; i++) if ((rising & (1 << i)) != 0) { heldMono.Remove(i); heldMono.Add(i); }

        int active = heldMono.Count > 0 ? heldMono[^1] : -1;
        if (active != monoButton || (chordChanged && active >= 0))
        {
            if (monoStreamId != 0) { instrument.Stop(monoStreamId); monoStreamId = 0; }
            if (active >= 0)
            {
                monoStreamId = instrument.Play(current[active], active, 0f);
                if (active != monoButton)
                    Console.WriteLine($"[play]   note {active}: {current[active].GetName()} (mono)");
            }
            monoButton = active;
        }
    }
    else
    {
        // Held-note swap on chord change: re-voice every still-held, still-sounding button whose note changed.
        if (chordChanged)
        {
            int sustained = lastMask & snap.NotesMask;
            for (int i = 0; i < liveButtons; i++)
                if ((sustained & (1 << i)) != 0 && streamId[i] != 0)
                {
                    int newPitch = current[i].GetPitch();
                    if (newPitch != playingPitch[i])
                    {
                        streamId[i] = instrument.Play(current[i], i, 0f);   // same-string mute steals the old voice
                        playingPitch[i] = newPitch;
                    }
                }
        }

        // Newly pressed buttons: pluck the current chord's note.
        for (int i = 0; i < liveButtons; i++)
            if ((rising & (1 << i)) != 0)
            {
                streamId[i] = instrument.Play(current[i], i, 0f);
                playingPitch[i] = current[i].GetPitch();
                Console.WriteLine($"[play]   note {i}: {current[i].GetName()}");
            }

        // Note-off: looping/sustained instruments stop when the button is released; plucked ones ring out.
        if (instrument.IsAutoLoop())
            for (int i = 0; i < liveButtons; i++)
                if ((falling & (1 << i)) != 0 && streamId[i] != 0)
                {
                    instrument.Stop(streamId[i]);
                    streamId[i] = 0;
                    playingPitch[i] = int.MinValue;
                }
    }

    if (rising != 0) drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.GetKey());

    lastMask = snap.NotesMask;
    if (drone != null && ++droneTick % 8 == 0) drone.OnTickUpdate(instrument.GetSoundPool());   // ~60 Hz glide
    Thread.Sleep(2);   // ~500 Hz input poll
}

instrument.Release();
Console.WriteLine("[play] bye.");
