using System.Linq;
using NAudio.Wave;
using VirtuoPhone;
using VirtuoPhone.Models;
using VirtuoPhone.Input;

Console.WriteLine("VirtuoPhone — play with the Mayflash F500 (buttons = notes, stick = chord).");

// Diagnostic mode (VP_DIAG=1): inspect the raw guitar low-E sample and play isolated notes — no polyphony,
// no chords, no glissando. Compare audio modes to isolate crackle: default (shared 48 kHz, resampled) vs
// VP_AUDIO_MODE=exclusive (44.1 kHz, bit-exact, no resample).
string? diagMode = Environment.GetEnvironmentVariable("VP_DIAG");   // "raw" / "engine" for audio debugging
if (diagMode != null)
{
    string oggPath = System.IO.Path.Combine(AppContext.BaseDirectory, "res", "raw", "guitare2.ogg");
    using (var reader = new NAudio.Vorbis.VorbisWaveReader(oggPath))
    {
        var samples = new List<float>();
        float[] buf = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
        int got;
        while ((got = reader.Read(buf, 0, buf.Length)) > 0)
            for (int k = 0; k < got; k++) samples.Add(buf[k]);
        float peak = 0f;
        foreach (float s in samples) peak = Math.Max(peak, Math.Abs(s));
        Console.WriteLine($"[diag] guitare2.ogg: {reader.WaveFormat.SampleRate} Hz, {reader.WaveFormat.Channels} ch, " +
                          $"{samples.Count} samples, peak abs = {peak:0.000}");
    }

    if (diagMode == "raw")
    {
        // Play guitare2.ogg straight through NAudio — engine fully bypassed — at a matched level & big buffer.
        using var reader = new NAudio.Vorbis.VorbisWaveReader(oggPath);
        var stereo = new NAudio.Wave.SampleProviders.MonoToStereoSampleProvider(reader);
        var quiet = new NAudio.Wave.SampleProviders.VolumeSampleProvider(stereo) { Volume = 0.41f };
        int rawLatency = int.TryParse(Environment.GetEnvironmentVariable("VP_LATENCY_MS"), out int rl) && rl > 0 ? rl : 100;
        using var wo = new NAudio.Wave.WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, true, rawLatency);
        wo.Init(quiet);
        Console.WriteLine($"[diag] RAW playback of guitare2.ogg via NAudio (engine bypassed, {rawLatency} ms buffer)…");
        wo.Play();
        Thread.Sleep((int)reader.TotalTime.TotalMilliseconds + 700);
    }
    else
    {
        Instrument diag = AppController.GetAppController().GetInstrument();   // SteelGuitar
        Console.WriteLine("[diag] ENGINE playback of isolated notes (listen for crackle, esp. the low E)…");
        foreach (int p in new[] { 28, 33, 40, 52 })   // E2 (low), A2, E3, E4
        {
            Console.WriteLine($"[diag]   {new Note(p).GetName()} (pitch {p})");
            diag.Play(p, 0, 0f);
            Thread.Sleep(2500);
        }
        Thread.Sleep(500);
        diag.Release();
    }
    return;
}

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

// 3×3 chord grid (Chromatic Spiral, CLAUDE.md Example 2), neutral center = E. Each cell is voice-led to the
// CENTER's voicing (closest inversion) and pre-rendered ONCE here — the joystick just looks up the result.
int[] centerVoicing = new Chord(Note.E, ChordType.maj).Select(n => n.GetPitch()).ToArray();

(int[] Voicing, int Root, string Name) MakeCell(int root, ChordType type)
{
    var chord = new Chord(root, type);
    var classes = new HashSet<int>();
    foreach (Note n in chord) classes.Add(n.GetPitch() % 12);
    return (VoiceLeading.ClosestVoicing(centerVoicing, classes), root % 12, chord.ToString());
}

var grid = new Dictionary<Direction, (int[] Voicing, int Root, string Name)>
{
    [Direction.UpLeft]    = MakeCell(Note.C,      ChordType.maj),
    [Direction.Up]        = MakeCell(Note.D,      ChordType.maj),
    [Direction.UpRight]   = MakeCell(Note.F,      ChordType.maj),
    [Direction.Left]      = MakeCell(Note.B,      ChordType.maj),
    [Direction.Neutral]   = MakeCell(Note.E,      ChordType.maj),
    [Direction.Right]     = MakeCell(Note.FSharp, ChordType.maj),
    [Direction.DownLeft]  = MakeCell(Note.ASharp, ChordType.maj),
    [Direction.Down]      = MakeCell(Note.A,      ChordType.maj),
    [Direction.DownRight] = MakeCell(Note.G,      ChordType.maj),
};

Direction dir = Direction.Neutral;
var current = grid[dir];
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
drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.Root);
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
Console.Write($"[play] neutral chord: {current.Name} — notes:");
for (int i = 0; i < liveButtons; i++)
    Console.Write($" b{i}={new Note(current.Voicing[i]).GetName()}({current.Voicing[i]})");
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
        drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.Root);

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
            if (monoButton >= 0) monoStreamId = instrument.Play(current.Voicing[monoButton], monoButton, 0f);
        }
        else
        {
            for (int i = 0; i < liveButtons; i++)
                if ((snap.NotesMask & (1 << i)) != 0)
                {
                    streamId[i] = instrument.Play(current.Voicing[i], i, 0f);
                    playingPitch[i] = current.Voicing[i];
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
        Console.WriteLine($"[play] chord -> {current.Name}  ({dir})");
        drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.Root);
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
                monoStreamId = instrument.Play(current.Voicing[active], active, 0f);
                if (active != monoButton)
                    Console.WriteLine($"[play]   note {active}: {new Note(current.Voicing[active]).GetName()} (mono)");
            }
            monoButton = active;
        }
    }
    else
    {
        // Held-note swap on chord change: re-voice every still-held, still-sounding button whose note changed.
        if (chordChanged)
        {
            float gliss = instrument.GetGlissandoSeconds();
            int sustained = lastMask & snap.NotesMask;
            for (int i = 0; i < liveButtons; i++)
                if ((sustained & (1 << i)) != 0 && streamId[i] != 0)
                {
                    int newPitch = current.Voicing[i];
                    if (newPitch != playingPitch[i])
                    {
                        if (gliss > 0f)
                            instrument.GlidePitch(streamId[i], playingPitch[i], newPitch, gliss);   // portamento
                        else
                            streamId[i] = instrument.Play(newPitch, i, 0f);   // re-attack (same-string mute steals old)
                        playingPitch[i] = newPitch;
                    }
                }
        }

        // Newly pressed buttons: pluck the current chord's note.
        for (int i = 0; i < liveButtons; i++)
            if ((rising & (1 << i)) != 0)
            {
                streamId[i] = instrument.Play(current.Voicing[i], i, 0f);
                playingPitch[i] = current.Voicing[i];
                Console.WriteLine($"[play]   note {i}: {new Note(current.Voicing[i]).GetName()}");
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

    if (rising != 0) drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.Root);

    lastMask = snap.NotesMask;
    if (drone != null && ++droneTick % 8 == 0) drone.OnTickUpdate(instrument.GetSoundPool());   // ~60 Hz glide
    Thread.Sleep(2);   // ~500 Hz input poll
}

instrument.Release();
Console.WriteLine("[play] bye.");
