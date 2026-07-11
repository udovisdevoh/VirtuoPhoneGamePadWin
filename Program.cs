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

// 3×3 chord grid (Chromatic Spiral, CLAUDE.md Example 2) as semitone OFFSETS from the center, so the whole
// layout can be transposed (Start-button modulation) just by moving the center. Each cell is voice-led to the
// current center's voicing (closest inversion); the grid is rebuilt whenever the center changes.
var offsets = new Dictionary<Direction, int>
{
    [Direction.UpLeft]    = 8,   // originally C (with center E)
    [Direction.Up]        = 10,  // D
    [Direction.UpRight]   = 1,   // F
    [Direction.Left]      = 7,   // B
    [Direction.Neutral]   = 0,   // E — the center
    [Direction.Right]     = 2,   // F#
    [Direction.DownLeft]  = 6,   // A#
    [Direction.Down]      = 5,   // A
    [Direction.DownRight] = 3,   // G
};

// Outer "dash" cells (double-tap a cardinal): tension chords beyond the 3×3 edges — placeholder dim7 voicings
// (cf. CLAUDE.md Example 0) until presets are editable. They transpose with the center like the inner cells.
var outerOffsets = new Dictionary<Direction, int>
{
    [Direction.Up]    = 8,   // Cdim7 with center E
    [Direction.Right] = 9,   // C#dim7
    [Direction.Down]  = 7,   // Bdim7
    [Direction.Left]  = 7,   // Bdim7
};

int centerRoot = Note.E;
bool centerIsMinor = false;

Dictionary<Direction, (int[] Voicing, int Root, string Name)> BuildGrid(int center, bool minorCenter)
{
    // Anchor = the center chord's own voicing; every cell is voice-led to it. The center goes minor on a
    // double-press modulation; all other cells stay major (the preset has no explicit per-cell quality yet).
    int[] anchor = new Chord(center, minorCenter ? ChordType.m : ChordType.maj).Select(n => n.GetPitch()).ToArray();

    var g = new Dictionary<Direction, (int[] Voicing, int Root, string Name)>();
    foreach (var (d, off) in offsets)
    {
        int root = (center + off) % 12;
        ChordType type = (d == Direction.Neutral && minorCenter) ? ChordType.m : ChordType.maj;
        var chord = new Chord(root, type);
        var classes = new HashSet<int>();
        foreach (Note n in chord) classes.Add(n.GetPitch() % 12);
        g[d] = (VoiceLeading.ClosestVoicing(anchor, classes), root % 12, chord.ToString());
    }
    return g;
}

Dictionary<Direction, (int[] Voicing, int Root, string Name)> BuildOuterGrid(int center, bool minorCenter)
{
    int[] anchor = new Chord(center, minorCenter ? ChordType.m : ChordType.maj).Select(n => n.GetPitch()).ToArray();
    var g = new Dictionary<Direction, (int[] Voicing, int Root, string Name)>();
    foreach (var (d, off) in outerOffsets)
    {
        int root = (center + off) % 12;
        var chord = new Chord(root, ChordType.dim7);
        var classes = new HashSet<int>();
        foreach (Note n in chord) classes.Add(n.GetPitch() % 12);
        g[d] = (VoiceLeading.ClosestVoicing(anchor, classes), root % 12, chord.ToString());
    }
    return g;
}

var grid = BuildGrid(centerRoot, centerIsMinor);
var outerGrid = BuildOuterGrid(centerRoot, centerIsMinor);

Direction dir = Direction.Neutral;
var current = grid[dir];
int lastMask = 0;
int liveButtons = Math.Min(strings, input.NoteButtonCount);
int[] playingPitch = new int[liveButtons];   // last pitch each button is sounding (polyphonic held-note swap)
Array.Fill(playingPitch, int.MinValue);
int[] streamId = new int[liveButtons];        // active stream per button (to stop looping notes on release)
int[] basePitch = new int[liveButtons];       // pitch each button's current voice was STRUCK at (glissando anchor)
Array.Fill(basePitch, int.MinValue);
bool lastSelect = false;
bool lastStart = false;
long lastStartMs = -10000;        // Start double-press timing (ms on the play stopwatch)
const long doubleTapMs = 300;     // Start double-press window
Direction? dashedDir = null;      // cardinal currently in "dash" (outer-cell) mode, or null
Direction? lastTapDir = null;     // last cardinal entered, for directional double-tap (dash) detection
long lastTapMs = -10000;
const long dashMs = 350;          // directional double-tap (dash) window

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
    : "[play] Play! (stick=chord, buttons=notes, Select=instrument, Start=modulate/dbl=minor, dbl-tap a dir=outer chord; Ctrl+C)");

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
        Array.Fill(basePitch, int.MinValue);
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
                    basePitch[i] = current.Voicing[i];
                }
        }
    }
    lastSelect = snap.Select;

    // Start-button modulation: a press transposes the grid so the aimed cell's chord becomes the new center
    // (single press → MAJOR center); a quick double-press then recolors that new center to MINOR (other cells
    // stay major). Non-blocking, timestamp-based double-tap detection; the 2nd tap only recolors (no re-transpose).
    bool gridChanged = false;
    if (snap.Start && !lastStart)
    {
        long nowMs = sw.ElapsedMilliseconds;
        bool doublePress = nowMs - lastStartMs <= doubleTapMs;
        lastStartMs = nowMs;

        if (doublePress)
            centerIsMinor = true;                                                            // recolor in place
        else
            (centerRoot, centerIsMinor) = ((centerRoot + offsets[snap.Dir]) % 12, false);    // transpose, major

        grid = BuildGrid(centerRoot, centerIsMinor);
        outerGrid = BuildOuterGrid(centerRoot, centerIsMinor);
        dashedDir = null;
        gridChanged = true;
        Console.WriteLine($"[play] MODULATE -> center {new Note(centerRoot).GetName()}{(centerIsMinor ? "m" : "")}"
                          + (doublePress ? "  (double->minor)" : $"  (via {snap.Dir})"));
    }
    lastStart = snap.Start;

    // Directional double-tap ("dash"): tapping a cardinal direction twice quickly (returning toward neutral
    // between taps) selects that direction's OUTER cell (an alternate tension chord) instead of its inner one.
    // The dash holds while the stick stays on that direction; leaving it reverts to the inner grid.
    bool dirChanged = snap.Dir != dir;
    if (dirChanged)
    {
        if (snap.Dir is Direction.Up or Direction.Down or Direction.Left or Direction.Right)
        {
            long nowMs = sw.ElapsedMilliseconds;
            dashedDir = (snap.Dir == lastTapDir && nowMs - lastTapMs <= dashMs) ? snap.Dir : null;
            lastTapDir = snap.Dir;
            lastTapMs = nowMs;
        }
        else
        {
            dashedDir = null;                                        // left the dashed cell
            if (snap.Dir != Direction.Neutral) lastTapDir = null;    // a diagonal breaks the tap chain; neutral keeps it
        }
        dir = snap.Dir;
    }

    // Chord in effect = the dashed outer cell, else the inner grid cell. Recompute on any direction/grid change.
    bool chordChanged = dirChanged || gridChanged;
    if (chordChanged)
    {
        bool dash = dashedDir == dir;
        current = dash ? outerGrid[dir] : grid[dir];
        if (!gridChanged) Console.WriteLine($"[play] chord -> {current.Name}  ({dir}{(dash ? " dash!" : "")})");
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
        // Held-note swap on chord change: re-voice every still-held, still-sounding button.
        if (chordChanged)
        {
            float gliss = instrument.GetGlissandoSeconds();
            int sustained = lastMask & snap.NotesMask;
            for (int i = 0; i < liveButtons; i++)
                if ((sustained & (1 << i)) != 0 && streamId[i] != 0)
                {
                    int newPitch = current.Voicing[i];
                    if (gliss > 0f)
                    {
                        // Portamento (violin, sitar): glide the ringing voice to the new note. Anchor on the
                        // pitch it was STRUCK at (basePitch), not the previous target, so E→F→E can't drift.
                        if (newPitch != playingPitch[i])
                        {
                            instrument.GlidePitch(streamId[i], basePitch[i], newPitch, gliss);
                            playingPitch[i] = newPitch;
                        }
                    }
                    else
                    {
                        // No portamento (piano, harp…): mute the previous note and re-strike this button on
                        // the new chord — always, even when the pitch is unchanged.
                        instrument.Stop(streamId[i]);
                        streamId[i] = instrument.Play(newPitch, i, 0f);
                        playingPitch[i] = newPitch;
                        basePitch[i] = newPitch;
                    }
                }
        }

        // Newly pressed buttons: pluck the current chord's note.
        for (int i = 0; i < liveButtons; i++)
            if ((rising & (1 << i)) != 0)
            {
                streamId[i] = instrument.Play(current.Voicing[i], i, 0f);
                playingPitch[i] = current.Voicing[i];
                basePitch[i] = current.Voicing[i];
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
