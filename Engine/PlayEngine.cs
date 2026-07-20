using System.Diagnostics;
using System.Linq;
using System.Threading;
using VirtuoPhone.Config;
using VirtuoPhone.Input;
using VirtuoPhone.Models;

namespace VirtuoPhone.Engine;

/// <summary>Live snapshot of what the engine is playing, pushed to the UI.</summary>
public readonly record struct EngineState(string Instrument, string Center, string Chord, Direction Direction, bool Dash, string Layout);

/// <summary>
/// The interactive play loop (formerly the console harness) on its own background thread: polls the controller,
/// resolves the active preset cell (with Start modulation + directional "dash"), and drives the instrument —
/// per-button polyphony, held-note re-voicing, monophony, drone, note-off. Never touches the UI thread; state
/// is pushed via <see cref="Status"/> / <see cref="StateChanged"/>. Preset and audio settings can be swapped live.
/// </summary>
public sealed class PlayEngine
{
    private sealed record GridCell(int[] Voicing, int Root, string Name);

    private readonly IControllerInput input;
    private Thread? thread;
    private volatile bool running;

    private volatile Preset preset;
    private volatile Preset? pendingPreset;
    private volatile AudioSettings? pendingAudio;
    private volatile int pendingOctaveShift = int.MinValue;   // int.MinValue = no pending shift
    private int startupInstrument;
    private int octaveSemitones;                 // global transpose applied to every voicing pitch
    private bool startOctaveHeld;                // Start button held = momentary +1 octave

    // Runtime tonal state.
    private int centerRoot;
    private bool centerMinor;
    private Dictionary<Direction, GridCell> grid = new();
    private Dictionary<Direction, GridCell> outerGrid = new();

    public event Action<string>? Status;
    public event Action<EngineState>? StateChanged;
    public bool IsRunning => running;

    public PlayEngine(IControllerInput input, Preset preset, int instrumentIndex, int octaveShift = 0)
    {
        this.input = input;
        this.preset = preset;
        startupInstrument = instrumentIndex;
        octaveSemitones = Math.Clamp(octaveShift, -2, 2) * 12;
    }

    public void SetPreset(Preset p) => pendingPreset = p;
    public void SetAudioSettings(AudioSettings a) => pendingAudio = a;
    public void SetOctaveShift(int octaves) => pendingOctaveShift = octaves;

    public void Start()
    {
        if (running) return;
        running = true;
        thread = new Thread(Run) { IsBackground = true, Name = "PlayEngine" };
        thread.Start();
    }

    public void Stop()
    {
        running = false;
        thread?.Join(1500);
        thread = null;
    }

    private void Log(string m) => Status?.Invoke(m);

    private int InnerOffset(Direction d) => ((preset.Inner[d].Root - preset.CenterRoot) % 12 + 12) % 12;
    private int OuterOffset(Direction d) => ((preset.Outer[d].Root - preset.CenterRoot) % 12 + 12) % 12;

    private GridCell MakeCell(int root, ChordType type, int[] anchor)
    {
        var chord = new Chord(root, type);
        int[] chordVoicing = chord.Select(n => n.GetPitch()).ToArray();
        int[] voicing = VoiceLeading.VoiceCell(anchor, chordVoicing);
        int shift = octaveSemitones + (startOctaveHeld ? 12 : 0);   // global transpose + momentary Start-held octave
        if (shift != 0)
            for (int i = 0; i < voicing.Length; i++) voicing[i] += shift;
        return new GridCell(voicing, root % 12, chord.ToString());
    }

    private void BuildGrids()
    {
        ChordType centerType = centerMinor ? ChordType.m : preset.Inner[Direction.Neutral].Type;
        int[] anchor = new Chord(centerRoot, centerType).Select(n => n.GetPitch()).ToArray();

        var g = new Dictionary<Direction, GridCell>();
        foreach (var d in Preset.AllInner)
        {
            int root = (centerRoot + InnerOffset(d)) % 12;
            ChordType type = (d == Direction.Neutral && centerMinor) ? ChordType.m : preset.Inner[d].Type;
            g[d] = MakeCell(root, type, anchor);
        }
        var og = new Dictionary<Direction, GridCell>();
        foreach (var d in Preset.Cardinals)
            og[d] = MakeCell((centerRoot + OuterOffset(d)) % 12, preset.Outer[d].Type, anchor);

        grid = g;
        outerGrid = og;
    }

    private void Run()
    {
        try { RunCore(); }
        catch (Exception e) { running = false; Log($"[engine] stopped on error: {e.Message}"); }
    }

    private void RunCore()
    {
        var app = AppController.GetAppController();
        Instrument instrument = app.SetInstrument(startupInstrument);
        int liveButtons = Math.Min(instrument.GetStringCount(), input.NoteButtonCount);

        centerRoot = preset.CenterRoot;
        centerMinor = false;
        BuildGrids();

        Direction dir = Direction.Neutral;
        GridCell current = grid[dir];
        long lastMask = 0;   // 64-bit: one bit per note button (up to 48)

        int[] playingPitch = new int[liveButtons]; Array.Fill(playingPitch, int.MinValue);
        int[] streamId = new int[liveButtons];
        int[] basePitch = new int[liveButtons];  Array.Fill(basePitch, int.MinValue);

        bool lastSelect = false, lastHome = false, lastStart = false;
        long lastHomeMs = -10000; const long doubleTapMs = 300;
        Direction? dashedDir = null, lastTapDir = null; long lastTapMs = -10000; const long dashMs = 350;

        var heldMono = new List<int>(); int monoButton = -1; int monoStreamId = 0;

        Drone? drone = instrument.GetDrone();
        drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.Root);
        int droneTick = 0;

        EngineState lastPushed = default;
        void PushState(bool dash)
        {
            string layout = string.Join(" ", Enumerable.Range(0, liveButtons).Select(i => new Note(current.Voicing[i]).GetName()));
            var st = new EngineState(app.InstrumentName, new Note(centerRoot).GetName() + (centerMinor ? "m" : ""),
                                     current.Name, dir, dash, layout);
            if (!st.Equals(lastPushed)) { lastPushed = st; StateChanged?.Invoke(st); }
        }
        PushState(false);

        // Re-arm all voice state on the current instrument and re-trigger held buttons (used after Select /
        // audio rebuild). Local function so both paths share it.
        void ReAttachInstrument(long notesMask)
        {
            liveButtons = Math.Min(instrument.GetStringCount(), input.NoteButtonCount);
            Array.Fill(playingPitch, int.MinValue);
            Array.Fill(streamId, 0);
            Array.Fill(basePitch, int.MinValue);
            heldMono.Clear(); monoButton = -1; monoStreamId = 0;
            drone = instrument.GetDrone();
            drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.Root);
            for (int i = 0; i < liveButtons; i++) if ((notesMask & (1L << i)) != 0) heldMono.Add(i);

            if (instrument.IsMonophonic())
            {
                monoButton = heldMono.Count > 0 ? heldMono[^1] : -1;
                if (monoButton >= 0) monoStreamId = instrument.Play(current.Voicing[monoButton], monoButton, 0f);
            }
            else
            {
                for (int i = 0; i < liveButtons; i++)
                    if ((notesMask & (1L << i)) != 0)
                    {
                        streamId[i] = instrument.Play(current.Voicing[i], i, 0f);
                        playingPitch[i] = basePitch[i] = current.Voicing[i];
                    }
            }
        }

        var sw = Stopwatch.StartNew();
        while (running)
        {
            // Apply live config swaps from the UI thread.
            if (pendingPreset is { } np)
            {
                pendingPreset = null;
                preset = np;
                centerRoot = preset.CenterRoot; centerMinor = false; dashedDir = null;
                BuildGrids();
                dir = Direction.Neutral; current = grid[dir];
                ReAttachInstrument(lastMask);
                Log($"[preset] {preset.Name}");
                PushState(false);
            }
            if (pendingAudio is { } na)
            {
                pendingAudio = null;
                AudioBackend.Settings = na;
                instrument = app.RebuildInstrument();
                ReAttachInstrument(lastMask);
                Log("[audio] settings applied");
            }
            if (pendingOctaveShift != int.MinValue)
            {
                int oct = pendingOctaveShift;
                pendingOctaveShift = int.MinValue;
                octaveSemitones = Math.Clamp(oct, -2, 2) * 12;
                BuildGrids();
                current = dashedDir == dir ? outerGrid[dir] : grid[dir];
                ReAttachInstrument(lastMask);
                Log($"[octave] {octaveSemitones / 12:+0;-0;0}");
                PushState(dashedDir == dir);
            }

            InputSnapshot snap = input.Poll();
            long rising = snap.NotesMask & ~lastMask;
            long falling = lastMask & ~snap.NotesMask;

            // Select → next instrument.
            if (snap.Select && !lastSelect)
            {
                instrument = app.NextInstrument();
                Log($"[instrument] {app.InstrumentName}");
                ReAttachInstrument(snap.NotesMask);
            }
            lastSelect = snap.Select;

            // Home modulation: single press transposes to the aimed cell (major center); a quick double-press
            // recolors that new center minor (no re-transpose).
            bool gridChanged = false;
            if (snap.Home && !lastHome)
            {
                long nowMs = sw.ElapsedMilliseconds;
                bool dbl = nowMs - lastHomeMs <= doubleTapMs;
                lastHomeMs = nowMs;
                if (dbl) centerMinor = true;
                else { centerRoot = (centerRoot + InnerOffset(snap.Dir)) % 12; centerMinor = false; }
                dashedDir = null;
                BuildGrids();
                gridChanged = true;
                Log($"[modulate] center {new Note(centerRoot).GetName()}{(centerMinor ? "m" : "")}" + (dbl ? " (double->minor)" : $" (via {snap.Dir})"));
            }
            lastHome = snap.Home;

            // Start held = momentary +1 octave: rebuild the grid an octave up and re-voice held notes through the
            // same path as a joystick chord change (glide or re-strike per instrument); releasing drops back down.
            if (snap.Start != lastStart)
            {
                startOctaveHeld = snap.Start;
                BuildGrids();
                gridChanged = true;
                Log($"[start-octave] {(startOctaveHeld ? "+1" : "0")}");
            }
            lastStart = snap.Start;

            // Directional dash: a cardinal tapped twice (D→neutral→D) within the window selects its outer cell.
            bool dirChanged = snap.Dir != dir;
            if (dirChanged)
            {
                if (snap.Dir is Direction.Up or Direction.Down or Direction.Left or Direction.Right)
                {
                    long nowMs = sw.ElapsedMilliseconds;
                    dashedDir = (snap.Dir == lastTapDir && nowMs - lastTapMs <= dashMs) ? snap.Dir : null;
                    lastTapDir = snap.Dir; lastTapMs = nowMs;
                }
                else
                {
                    dashedDir = null;
                    if (snap.Dir != Direction.Neutral) lastTapDir = null;
                }
                dir = snap.Dir;
            }

            bool chordChanged = dirChanged || gridChanged;
            bool dash = dashedDir == dir;
            if (chordChanged)
            {
                current = dash ? outerGrid[dir] : grid[dir];
                drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.Root);
                Log($"[chord] {current.Name} ({dir}{(dash ? " dash!" : "")})");
            }

            if (instrument.IsMonophonic())
            {
                for (int i = 0; i < liveButtons; i++) if ((falling & (1L << i)) != 0) heldMono.Remove(i);
                for (int i = 0; i < liveButtons; i++) if ((rising & (1L << i)) != 0) { heldMono.Remove(i); heldMono.Add(i); }

                int active = heldMono.Count > 0 ? heldMono[^1] : -1;
                if (active != monoButton || (chordChanged && active >= 0))
                {
                    if (monoStreamId != 0) { instrument.Stop(monoStreamId); monoStreamId = 0; }
                    if (active >= 0) monoStreamId = instrument.Play(current.Voicing[active], active, 0f);
                    monoButton = active;
                }
            }
            else
            {
                if (chordChanged)
                {
                    float gliss = instrument.GetGlissandoSeconds();
                    long sustained = lastMask & snap.NotesMask;
                    for (int i = 0; i < liveButtons; i++)
                        if ((sustained & (1L << i)) != 0)   // held button — re-voice even if it fell silent (streamId 0)
                        {
                            int newPitch = current.Voicing[i];
                            if (gliss > 0f && streamId[i] != 0)
                            {
                                if (newPitch != playingPitch[i])
                                {
                                    instrument.GlidePitch(streamId[i], basePitch[i], newPitch, gliss);
                                    playingPitch[i] = newPitch;
                                }
                            }
                            else
                            {
                                // Non-portamento, OR a held voice that went silent (a cell put it below minPitch):
                                // re-strike on the new chord. Doing this regardless of streamId is what lets a note
                                // come back after an out-of-range cell — otherwise streamId stays 0 and it's muted forever.
                                if (streamId[i] != 0) instrument.Stop(streamId[i]);
                                streamId[i] = instrument.Play(newPitch, i, 0f);
                                playingPitch[i] = basePitch[i] = newPitch;
                            }
                        }
                }

                for (int i = 0; i < liveButtons; i++)
                    if ((rising & (1L << i)) != 0)
                    {
                        streamId[i] = instrument.Play(current.Voicing[i], i, 0f);
                        playingPitch[i] = basePitch[i] = current.Voicing[i];
                    }

                if (instrument.IsAutoLoop())
                    for (int i = 0; i < liveButtons; i++)
                        if ((falling & (1L << i)) != 0 && streamId[i] != 0)
                        {
                            instrument.Stop(streamId[i]); streamId[i] = 0; playingPitch[i] = int.MinValue;
                        }
            }

            if (rising != 0) drone?.OnPlayNoteUpdate(instrument.GetSoundPool(), current.Root);
            if (chordChanged || snap.Select) PushState(dash);

            lastMask = snap.NotesMask;
            if (drone != null && ++droneTick % 8 == 0) drone.OnTickUpdate(instrument.GetSoundPool());
            Thread.Sleep(2);
        }

        instrument.Release();
    }
}
