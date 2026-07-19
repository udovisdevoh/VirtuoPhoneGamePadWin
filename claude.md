# Project Context: Arcade Stick Music Synthesizer (VirtuoPhone GamePad — Windows)

## 1. Project Overview
A Windows app (C#/.NET 8) that turns a video-game controller — tuned for arcade fight sticks
like the **Mayflash F500 Elite** — into a polyphonic musical instrument.

- **Note buttons (up to 48)** → play the notes of the currently selected scale/chord; the F500's 8 buttons
  cover the lowest 8, the keyboard extends to 48 for a wide melodic range (best for scales).
- **Joystick (left)** → selects the active chord/scale via a **3×3 matrix** (neutral center + 8 directions),
  with optional outer double-tap ("dash") positions.

This is a **port of an existing Android app** (`VirtuoPhone`). The audio-output layer is being
migrated Java → C#; the input, UI, and configuration layers are being (re)built for Windows.

**Reference Java source (audio engine):** `..\..\Java\VirtuoPhone\src\com\virtuophone`

---

## 2. Implementation Status ⚠️ READ FIRST
The design below is largely **intended behavior**. The current code reflects only part of it.

| Area | Status | Notes |
|------|--------|-------|
| Audio domain model (Instrument, Chord, Note, Drone, Sample, StringExpander…) | ✅ Ported from Java | See §4–§5 |
| 9 instruments + ~110 `.ogg` samples in `res/raw/` | ✅ Present | Banks: guitar, bagpipes, harp, harpsichord, piano, sitar(+drone/tampura), synth, violin, jew's-harp (`guimb*`) |
| Low-latency audio engine (SoundPool-equivalent) | ✅ **Working** | `Audio/NAudioSoundPool.cs` — NAudio `MixingSampleProvider` + WASAPI shared (~10 ms), polyphonic, per-voice pitch/envelope/glissando; behind an `ISoundPool` seam. `DummySoundPool` kept as a silent/headless backend |
| **Project compiles & plays audio** | ✅ **Yes** | `R` class generated; NAudio + NAudio.Vorbis added; plays clean polyphony — no pops or crackle (verified by ear) |
| Controller / gamepad + keyboard input | ✅ **Working** | `Input/MappedControllerInput.cs` reads the winmm gamepad (F500) **and** keyboard (`KeyboardReader`) through a remappable `ControllerMap`; `WinmmControllerInput` kept for the raw F500 mapping |
| UI | ✅ **WinForms app** | `UI/MainForm.cs` — tabs: Play (live state), Presets (grid editor), Controls (remap), Audio. The play loop is `Engine/PlayEngine.cs` on a background thread (`VP_HEADLESS=1` runs it console-only) |
| JSON presets & config | ✅ **Working** | `Config/*` — presets (grid cells), controller map, and audio settings persist as JSON at `%APPDATA%/VirtuoPhone/config.json` (`ConfigStore`) |
| Modulation / dash / octave / configurable audio | ✅ **Working** | Home-modulation + directional dash + hold-Start momentary octave + global octave ± transpose in `PlayEngine`; audio device/mode/latency/volume via `AudioSettings` → `NAudioSoundPool`. Pitch-bend-on-move (§6) is the last optional TODO |

### Resolved — `R.Raw.*` resource ids (was the initial build blocker)
**Fixed:** `Resources/R.cs` is auto-generated — a nested `R.Raw` of int constants plus an
`id → res/raw/<name>.ogg` map exposed via `R.GetFileName(id)`, produced from the union of code references
and bundled files. Every instrument resolves and the project builds; regenerate it when the sample set
changes. `NAudioSoundPool` uses `R.GetFileName(id)` to locate each `.ogg`.

**How sample resources are loaded in the C# port is an open design decision** — it does **not** have to
copy the Android `R.raw` scheme, and the `res/raw/*.ogg` files are only a loose starting point that may
be renamed, reorganized, or replaced. Two directions:
- **Fastest unblock:** add a small `R.Raw` shim (constants matching the current call sites —
  `guitare2`, `harpa0`, `guimb1`, …) so the existing instrument code compiles unchanged.
- **Cleaner redesign:** drop `R.Raw.*` in favor of whatever suits the real engine — file paths, an enum,
  a resource manager/dictionary, embedded resources — and update the instrument call sites to match.

Whichever is chosen, `Sample` / `DummySoundPool.Load(...)` / the real engine must simply agree on what a
"resource id" is (int handle, path, or key). Don't treat the current Android-style names as a constraint.

---

## 3. Tech Stack & Architecture
- **Language / runtime:** C# on **.NET 8** — **`net8.0-windows` + WinForms** (`UseWindowsForms=true`,
  `ImplicitUsings=enable`, `Nullable=enable`). `OutputType=Exe` keeps a console for logs / `VP_HEADLESS`.
- **UI:** **WinForms** (`UI/MainForm.cs`, tabbed) — chosen for simple-to-maintain + native/performant. The core
  (models/audio/input/engine) stays UI-agnostic; the **`PlayEngine`** runs the play loop on its own thread and
  pushes state to the UI via events (never blocks the UI or audio).
- **Configuration:** human-readable **JSON** at `%APPDATA%/VirtuoPhone/config.json` — presets, controller map,
  audio settings (`Config/*`, `System.Text.Json` with string enums). **Built** (`ConfigStore.Load/Save`).
- **Audio Engine:** custom low-latency C# engine modeled on Android `SoundPool` (pre-loaded buffers, real-time
  polyphonic playback, per-stream rate/volume); real backend `NAudioSoundPool` (device/mode/latency/volume from
  `AudioSettings`), `DummySoundPool` for headless/tests.
- **Namespaces:** `VirtuoPhone` (root: `AppController`, audio seam), `VirtuoPhone.Models`, `VirtuoPhone.Input`,
  `VirtuoPhone.Config`, `VirtuoPhone.Engine`, `VirtuoPhone.UI`. Files in `Audio/` use the root `VirtuoPhone`.

### Decisions (2026-07-10)
- **Approach:** build a **vertical slice first** — one instrument + one hard-coded preset + the real F500 → sound —
  and measure end-to-end latency before adding breadth.
- **Audio (built):** **`NAudioSoundPool`** = NAudio + NVorbis, behind **`ISoundPool`** (swappable via
  `AudioBackend`). **Mixing + output are NAudio's own `MixingSampleProvider` + `WasapiOut`** — we do **not**
  hand-roll the mixer (see the lesson below). Each playing note is a **`VoiceSampleProvider`** (an
  `ISampleProvider` that reads its cached mono sample at a variable rate — pitch via **cubic/Catmull-Rom**
  interpolation, optional envelope, glissando) added as a mixer input; it returns 0 at end-of-stream so the
  mixer drops it. A **`VolumeSampleProvider`** master applies per-poly headroom.
  Source samples are **mono 44.1 kHz**; each is **resampled once, in RAM, at load** (cubic) to the
  output/device rate — so the **playback path never resamples** (only genuine pitch shifts interpolate) and,
  in shared mode, the OS doesn't resample the mix every buffer. An unshifted note plays at step 1.0.
  **Output defaults to WASAPI _shared_ at the device mix rate** so the **Windows volume slider works**,
  attacks aren't affected by exclusive stream start/stop, and other apps' audio coexists. Set
  **`VP_AUDIO_MODE=exclusive`** for **44.1 kHz bit-exact**, lowest latency — but it **bypasses the Windows
  volume mixer** and takes the device exclusively. **Do not resample/filter on the _playback_ path, and do
  not add a *global* attack/release envelope** (it smears the pluck transient — the user rejected both; the
  per-instrument envelope in §6 is opt-in). Resampling **once at load** is the sanctioned way to match the
  device rate.
- **Audio lesson (hard-won — don't reinvent the mixer):** an earlier **hand-rolled mixer** (a custom
  `ISampleProvider` summing voices, fed straight to `WasapiOut`) produced a persistent **crackle in shared
  mode** that bare NAudio playback did not — empirically isolated (it was **not** the lock, cubic interp,
  limiter, samples, or resampling; it was the custom mix stage itself). **Fix: use NAudio's
  `MixingSampleProvider`.** Don't hand-roll audio mixing when the library's is proven. Keep any render-thread
  code **allocation-free** (no per-buffer enumerator/LINQ) so the GC can't pause it into underruns. Earlier
  low-latency "pops" were **buffer underruns**: 5 ms underran; **10 ms is clean and punchy**; to go lower,
  add a custom render thread on **MMCSS "Pro Audio"**. Audio choices now live in the **Audio settings tab**
  (`AudioSettings` in the JSON config), not env vars; **`VP_HEADLESS=1`** runs the engine console-only.
- **Input (built):** **`MappedControllerInput`** reads the winmm gamepad (F500) **and** the keyboard uniformly
  behind **`IControllerInput`**, resolving both through a remappable **`ControllerMap`** (the **Controls** tab
  rebinds any action to a gamepad button and/or a key; persisted in the JSON config).
  *(Dev/test device: a Mayflash F500 Elite is connected via USB.)*
- **UI (built):** **WinForms** (`UI/MainForm.cs`), tabbed (Play / Presets / Controls / Audio). The core stays
  decoupled — the `PlayEngine` runs off-thread and pushes state via events.
- **Resource ids (`R.Raw.*`):** resolved with an **auto-generated `R` class** (int ids → `res/raw/<name>.ogg`),
  generated from the union of code references and bundled files; regenerate when samples change.

---

## 4. Codebase Map
```
VirtuoPhoneGamePadWin.csproj   .NET 8 (net8.0-windows) WinForms Exe
Program.cs                     Entry point ([STAThread] Main): launches UI/MainForm; VP_HEADLESS=1 runs the engine console-only
AppController.cs               Singleton; STRING_COUNT=48 (note buttons per voicing); instrument factory + cache; Next/Set/RebuildInstrument()
Audio/
  ISoundPool.cs                SoundPool API surface — the swappable audio seam
  NAudioSoundPool.cs           Real engine: NVorbis decode, load-time cubic resample, NAudio MixingSampleProvider + WASAPI
  VoiceSampleProvider.cs       One playing voice as an ISampleProvider (variable-rate read, envelope, glissando) — a mixer input
  DummySoundPool.cs            Silent/headless ISoundPool (tests / no audio device)
  AudioBackend.cs              Factory selecting the ISoundPool backend (default NAudio) + current AudioSettings
  AudioDevices.cs              Lists WASAPI render endpoints for the Audio settings device picker
  PointerMemory.cs             Tracks active stream IDs so they can be stopped together
Input/
  IControllerInput.cs          Controller abstraction (gamepad/keyboard uniform): Poll() → InputSnapshot (notes mask, Direction, Start, Select, Home)
  WinmmJoystick.cs             Low-level winmm P/Invoke (joyGetPosEx/joyGetDevCaps) — raw buttons + axes
  WinmmControllerInput.cs      IControllerInput over WinmmJoystick: F500 button→note mask + 8-way stick → Direction
  KeyboardReader.cs            Polled key state via GetAsyncKeyState (fits the engine poll loop)
  MappedControllerInput.cs     IControllerInput combining gamepad + keyboard through a remappable ControllerMap
Models/
  Note.cs                      Pitch as semitone int; C..B constants; pitch = noteType + octave*12
  Chord.cs                     Hard-coded voicings per ChordType; transpose; (de)serialize "name:typeId:noteType"
  ChordType.cs                 enum: maj/min/7ths/9ths/sus/dim/aug/pentatonics/"ff_" sets/7 modes/harmonic&melodic-minor scales…
  GuitarPreset.cs              Ordered List<Chord>; indexed by Point(X=string, Y=chord); serialize by lines
  Sample.cs                    One .ogg mapped to an original pitch; 12-TET rate = 1.0594632^(Δsemitone+bend)
  MultiSampleSet.cs            Multiple samples for one pitch; GetRandomSample() for round-robin variation
  StringExpander.cs            Grow/shrink a chord's note count to match instrument string count
  VoiceLeading.cs              Closest-inversion re-voicing (minimal movement, keeps all chord tones)
  Drone.cs                     Sustained bass that follows chord roots (glide, lazy-harmonic, min-pitch-shift)
  PlayingStringMemory.cs       Per-string → stream-id map (mute previous note on same string)
  Instruments/
    Instrument.cs              Abstract base: preloads samples into multiSampleList[128], interpolates gaps,
                               Play/Stop/SetStreamPitch, feature flags via abstract Build*() methods
    Bagpipes, Harp, Harpsichord, JewsHarp, Piano, Sitar, SteelGuitar, Synth, Violin  (9 concrete instruments)
Config/
  AudioSettings.cs             Device / shared-vs-exclusive / latency / master volume (→ NAudioSoundPool)
  Preset.cs                    A preset: 9 inner cells + 4 outer "dash" cells, each Cell(root, ChordType); Default() = Chromatic Spiral
  ControllerMap.cs             Remappable gamepad-button + keyboard-key bindings per action (F500 + keyboard defaults)
  AppConfig.cs                 Presets + active + instrument + octave-shift + audio + controls; ConfigStore load/save JSON (%APPDATA%/VirtuoPhone)
Engine/
  PlayEngine.cs                The play loop on a background thread: modulation, dash, voice management; Status/StateChanged events
UI/
  MainForm.cs                  WinForms window + tabs; owns config/input/PlayEngine; Play tab has a global octave ± transpose
  PresetTab.cs                 Grid editor (edit each cell's chord) + preset new/duplicate/rename/delete (auto-saves; no Save button)
  ControlsTab.cs               Remap table; rebind any action to a gamepad button and/or a key
  AudioTab.cs                  Output device / mode / latency / master-volume, live-applied
  Capture.cs, Prompt.cs        Small modal dialogs (key/button capture, text input)
Resources/R.cs                 Auto-generated resource-id table (R.Raw.* -> res/raw/<name>.ogg); see §2
res/raw/*.ogg                  ~110 mono 44.1 kHz samples; copied next to the exe at build (see .csproj)
claude.original.prompt.md      Original French brief that seeded this project (design intent)
```

---

## 5. Domain Model & Conventions
Read these before touching audio code — they are the load-bearing invariants of the Java port.

- **Pitch = integer semitones (MIDI-like).** `pitch = noteType + octave*12`, `noteType` 0..11 (`C`=0 … `B`=11).
  `Note` exposes named constants (`Note.E`, `Note.FSharp`, …). Sub-zero / >127 pitches are clamped or skipped.
- **Pitch-shifting a sample:** `rate = 1.0594632^((desiredPitch - originalPitch) + pitchBend)`
  (12-tone equal temperament; `1.0594632 ≈ 2^(1/12)`). Playing a note = find the `MultiSampleSet` at that
  MIDI index, pick a (random) `Sample`, play it at the computed `rate`.
- **`Instrument` base class** (constructor does the heavy lifting):
  - Builds a `MultiSampleSet[128]` indexed by MIDI pitch; **`InterpolateBlankSamples()`** fills empty pitches
    from the nearest lower sample so every pitch is playable from a sparse recorded set.
  - Polyphony = `stringCount * 2`.
  - **`stringCount` = the number of note buttons = `AppController.STRING_COUNT` = 48 for every instrument**
    (chords/scales are voiced / `StringExpander`-ed to 48 ascending notes). Grown 8 → 21 → 48 so the keyboard
    plays a wide range — the F500's 8 physical buttons cover the lowest 8 note slots, the rest are keyboard. A
    48-note voicing spans `ceil(48/classCount)` octaves (a 7-note scale ≈ 7 octaves ≈ fits; a triad ≈ 16 octaves).
    `StringExpander` stacks tight ascending (next chord tone above the top) then octave-shifts the whole voicing
    so the bottom is ≥ 0; **few-note chords run past MIDI 127 at the top and the engine wraps those at play**
    (48 distinct notes only fit for dense targets — 48 buttons is aimed at scales). The **`InputSnapshot.NotesMask`
    is a 64-bit `long`** (48 > 32 bits).
  - Behavior is declared by overriding abstract **`Build*()`** flags:
    `BuildStringCount`, `BuildMinPitchToPlay`, `BuildIsMuteOnChangeFretSameString`, `BuildIsAutoLoop`,
    `BuildIsAutoLoopKeepNoteUntilNewNote`, `BuildIsPitchBend`, `LoadDrone`,
    `BuildIsLazyHarmonicDrone`, `BuildIsDroneMinimizePitchShift`, and `LoadSamples`.
  - Loop semantics passed to `SoundPool.Play(..., loop, rate)`: `loop = -1` = infinite (auto-loop/sustained
    instruments), `loop = 0` = one-shot.
- **`SoundPool` API surface** to preserve when writing the real engine (mirror Android):
  `int Load(resourceId, priority)` · `int Play(soundId, lVol, rVol, priority, loop, rate)` · `Stop(streamId)`
  · `SetVolume(streamId, l, r)` · `SetRate(streamId, rate)` · `Release()`. IDs returned by `Play` are stream
  handles used for later pitch-bend/stop.
- **`Chord`** stores fixed, guitar-shaped voicings per `ChordType`, transposable via `SetFundamental()`
  (keeps fundamental within a fixed register window), with octave de-duplication. `StringExpander`
  then adds notes (duplicating the rarest note type, up an octave) or removes them (dropping the most
  extreme instance of the most frequent note type) to hit the instrument's `stringCount`.
- **`GuitarPreset`** = ordered `List<Chord>` addressed by **`Point(X = string/button, Y = chord/position)`**.
  The mapping "joystick direction → `Y` index" is a **planned** input-layer convention, not yet coded.
- **Serialization (current, NOT JSON):**
  - `Chord` → `"<name>:<chordTypeId>:<noteType>"`
  - `GuitarPreset` → first line = name, then one serialized chord per line (`\r`/`\n` separated).
  - The move to JSON presets/config (§7) is still to do; keep round-tripping compatible or write a converter.

---

## 6. Input Handling & Hardware Mechanics (target design — not yet implemented)
Must be highly responsive: crisp execution, rapid directional inputs, low-latency polling, and it must
**never block the audio thread**.

- **Note buttons (up to 48):** trigger notes of the active scale/chord (the F500's 8 physical buttons cover the
  lowest 8 note slots; the keyboard's `ControllerMap.NoteCount`=48 keys extend the range); per-button polyphony.
  The note bitmask is a 64-bit `long` (48 buttons exceed a 32-bit int).
- **Joystick (left):** selects tonal center / active preset cell via a 3×3 grid.
  - **Neutral:** the root chord/scale (e.g. E).
  - **8 directions:** instantly shift the active chord/scale per the loaded preset.
- **Dynamic pitch-bend / sample-swap:** if a button is **held** while the joystick moves, the sounding note
  pitch-bends or swaps to the corresponding sample in the new chord/scale (see `Instrument.SetStreamPitch`,
  `IsPitchBend`).
- **Directional double-tap ("dash"):** ✅ quickly tapping a cardinal direction twice (D→neutral→D within
  ~350 ms) selects that direction's **outer cell** — an alternate tension chord beyond the 3×3 edge. The dash
  holds while the stick stays on that direction and reverts on leaving it. Outer cells are offsets from the
  center (they transpose with modulation); currently placeholder **dim7** voicings until presets are editable.
- **System buttons:**
  - **Select:** cycle loaded instruments (sample banks). ✅
  - **Home (modulation):** ✅ the grid is stored as semitone **offsets from the center**; a Home press moves it:
    - *Single press (+ aimed direction):* transpose so the aimed cell's chord becomes the new center (MAJOR);
      every cell shifts by the same interval. Pressing at neutral resets the center to major.
    - *Double-press (within ~300 ms):* recolor the just-established center to **minor** in place — no extra
      transpose; other cells stay major. Non-blocking, timestamp-based detection off the audio path.
  - **Start (hold = +1 octave):** ✅ while held, every voicing is rebuilt an octave up and held notes re-voice
    through the **same path as a joystick chord change** (glide or re-strike per instrument); releasing drops
    back down. Stacks on the persistent UI octave transpose. *(Modulation used to be on Start; it moved to Home.)*
- **Remappable controls:** a **remap menu** lets every action be rebound to a controller button/axis **or a
  keyboard key** (keyboard and gamepad are interchangeable input sources), persisted in the JSON config.

### Voice management & note lifecycle (implemented in the play harness `Program.cs`)
How held buttons + joystick chord changes map onto engine voices:
- **Per-button polyphony.** ✅ Each action button owns its own voice; re-pressing/swapping steals *that
  button's* voice (via the instrument's same-string mute). Polyphony is per button, not global.
- **Chord change while holding.** ✅ When the joystick changes the chord, each held button is re-voiced
  immediately (no re-press). **Portamento** instruments (violin, sitar) glide the ringing voice to the new
  note; **all others** (piano, harp…) mute the previous note and re-strike the button on the new chord —
  always, even if that button's pitch is unchanged.
- **Per-instrument note-off.** ✅ On release, **looping/sustained** instruments (`IsAutoLoop()`: violin,
  bagpipes) **stop**; **plucked** ones (guitar, piano, harp, harpsichord, sitar, jew's-harp, synth) ring out.
- **Monophonic instruments** (`IsMonophonic()`: bagpipes chanter). ✅ **Last-note priority with fall-back**:
  a new press replaces the sounding note; releasing it falls back to the newest still-held button (a held
  stack in the harness). The drone is separate, so bagpipes is effectively "bi-phonic".
- **Optional per-voice envelope.** ✅ `Instrument.BuildAttackSeconds()`/`BuildReleaseSeconds()` (default 0 =
  hard edge) drive a per-voice attack/release fade in `NAudioSoundPool`. Only the **violin** sets them > 0.
  This is **opt-in per instrument** — do not apply an envelope globally (an always-on envelope was rejected).
- **Optional glissando (portamento).** ✅ `Instrument.BuildGlissandoSeconds()` (default 0 = re-attack) makes
  a **held** voice **glide** to the new note on a chord change instead of re-triggering — `Instrument.GlidePitch`
  → `ISoundPool.GlideRate` slews the voice's rate over that time. The **violin** and the **sitar** (meend) set
  it > 0. The glide is **anchored to the pitch the voice was struck at** (`VoiceSampleProvider.baseRate`), not
  the previous target, so chained changes (E→F→E) can't drift.
- **Drone instruments.** ✅ **sitar** (C# tampura) and **bagpipes** (A) add a sustained root that tracks the
  chord (the `Drone` class via `LoadDrone`/`setDrone`; lazy-harmonic). On a chord change the drone **glides
  sample-accurately** to the new root via `ISoundPool.GlideToRate` (smooth — an earlier ~60 Hz per-tick
  stepping sounded stepped). The legacy `pitchAdjustSpeedMultiplicator` now just sets the glide **duration**
  (bagpipes **1.1** ≈ quick; sitar **1.2** ≈ near-instant). Harp, harpsichord, steel guitar have no drone. A
  **`BuildIsDroneFixed()`** flag exists (default false, **unused/dormant**) to hold a drone fixed against
  joystick chords — kept for a future instrument.

---

## 7. Configuration & Presets
Presets define the 3×3 musical matrices (with optional outer cells for double-tap). The user must be able
to **create, edit, save, load, and select** presets (chord layouts + their per-cell chords) at runtime,
plus a strictly-defined, remappable controller map. Target format is **human-readable JSON**.
*(Today only the text serialization in §5 exists; JSON load/save and the editor are unbuilt — see §8 steps 5–6.)*

Matrices are a 3×3 grid centered on the neutral joystick position, with optional outer extensions.

**Example 0 — E pentatonic minor (EGABD), stable corners, center E is tonic (try this first; outer = double-tap):**
```
           |Cdim7|
      | B  |  Bb | A  |
|Bdim7| C  |  E  | F  |C#dim7|
      | D  |  F# | G  |
           |Bdim7|
```

**Example 1 — Extended layout (double-tap outer bounds):**
```
           | B+ |
       | D | F# | G |
|F#dim7| F | E  | B |C#dim7|
       | C | Bb | A |
           | E+ |
```
*(Center E; diagonals F#, G, A, Bb; outer edges require double-tap.)*

**Example 2 — Chromatic Spiral**
```
| C  | D  | F  |
| B  | E  | F# |
| Bb | A  | G  |
```

**Example 3 — Chromatic Spiral (Circle of 5ths)**
```
| F# | B  | A  |
| Bb | E  | D  |
| F  | C  | G  |
```

**Example 4 — Minor Blues Rotation (tensions on sides, stable on diagonals)**
```
| B  | C  | D  |
| Bb | E  | F# |
| A  | F  | G  |
```

**Example 5 — Minor Blues Rotation (tensions on diagonals, stable on sides)**
```
| C  | D  | F# |
| B  | E  | G  |
| Bb | A  | F  |
```
*(Underlying set: F# B E A D G C F Bb)*

**Example 6 — Pure Circle of 4ths Rotation**
```
| C  | G  | D  |
| F  | E  | A  |
| Bb | Eb | Ab |
```

**Example 7 — Center dim7 (most unstable center, stable corners)**
```
| C  | Bb | D# |
| G  | Bo | C# |
| A  | E  | F# |
```

---

## 8. Roadmap / TODO (suggested order)
1. ✅ **Unblock the build** — `Resources/R.cs` generated; project compiles.
2. ✅ **Audio engine** — `NAudioSoundPool`: NAudio's `MixingSampleProvider` + WASAPI shared (~10 ms),
   polyphonic, per-voice pitch/envelope/glissando (`VoiceSampleProvider`), voice cap + master headroom,
   behind `ISoundPool`; swappable via `AudioBackend`. `DummySoundPool` kept as the headless backend.
3. ✅ **Prove audio end-to-end** — `Program.cs` plays chords through the instrument: no clipping, and **no
   pops or crackle at ~10 ms shared** (verified by ear, after switching to NAudio's mixer — see §3 lesson).
4. ✅ **Controller input** — `Input/MappedControllerInput.cs` reads the F500 (winmm) **and** the keyboard
   behind `IControllerInput` through a remappable `ControllerMap`; polled off the audio thread.
5. ✅ **Preset persistence (JSON)** — `Config/*` + `ConfigStore`: presets (grid cells), controller map, and
   audio settings load/save as JSON at `%APPDATA%/VirtuoPhone/config.json`. (The §5 text serialization on
   `Chord`/`GuitarPreset` remains for the Java-port model; the app itself uses the JSON config.)
6. ✅ **Preset editing** — the **Presets** tab: set each cell's chord (root + `ChordType`) and
   new / duplicate / rename / delete / select presets; edits push live to the engine and persist.
7. **Interaction logic & voice management:** ✅ done — per-button polyphony, held-note swap on chord change,
   per-instrument note-off, monophony (last-note priority + fall-back), optional violin envelope, drones
   (bagpipes/sitar), Select = change instrument, **Home = modulation**, **hold-Start = +1 octave**, and the
   **double-tap "dash"**. The §6 mechanics are complete except the optional **pitch-bend-on-move** (a held note
   bends continuously as the stick moves — distinct from the discrete octave/chord re-voicings). See §6.
8. ✅ **UI** — `UI/MainForm.cs` (WinForms), tabs Play / Presets / Controls / Audio; the `PlayEngine` runs
   off-thread and pushes live state to the Play tab.
9. ✅ **Configurable audio** — the **Audio** tab / `AudioSettings`: output **device**, **shared vs exclusive**,
   **latency**, **master volume**, live-applied and persisted (replaces the old `VP_*` env vars). Defaults stay
   WASAPI **shared** at the device rate, ~10 ms. **Remaining (optional):** an MMCSS "Pro Audio" render path.
10. ✅ **Voice leading** — `Models/VoiceLeading.cs`. `ClosestVoicing` re-voices the target as the voicing of
    **minimal total movement** from the centre voicing under two hard rules: **every chord tone present** and
    **strictly ascending** (each voice higher than the last — this also bans exact duplicates and gives a clean
    low→high layout). A small **dynamic program** evaluates every nearby inversion (widening the candidate window
    until a complete ascending voicing fits) and picks the closest; ties prefer the higher pitch. `VoiceCell`
    adds the policy the engine uses: **re-voice only when the centre and target chords have the same number of
    notes**; when they differ (triad centre vs a 7th / pentatonic / dim7 / scale cell) the re-organisation is
    **skipped** and the target chord's own voicing is used (sorted ascending), avoiding the awkward non-ascending
    spreads remapping would force. **Pre-rendered once** per preset. E.g. E major → C = `E C E G C E G C`;
    E → Bb = `F A# F A# D F A# D` (ascending). Verified by unit tests (E→C/A/F/B/Am, C→E, E→Bb, E→D, and the
    pentatonic same-count / different-count cases).

---

## 9. Build & Run
```powershell
dotnet build                                 # net8.0-windows (WinForms)
dotnet run                                   # launches the WinForms app (Play / Presets / Controls / Audio tabs)
dotnet test Tests/VirtuoPhone.Tests.csproj   # unit tests (voice-leading, string-expander, chord types, config JSON)
$env:VP_HEADLESS=1; dotnet run               # headless: run the engine with console logging, no window
```
Target framework is `net8.0-windows` (WinForms). Config persists to `%APPDATA%/VirtuoPhone/config.json`.
When adding NuGet packages, add them to `VirtuoPhoneGamePadWin.csproj`.

---

## 10. Development Directives for the AI
- **Prioritize low latency:** mirror Android `SoundPool` — pre-load buffers, real-time polyphony, no dropouts.
  Keep the API surface in §5 so the ported model classes don't need rewriting.
- **Never block the audio thread:** input polling, double-tap timing windows, and UI updates run off it.
- **Relative interval math for modulation:** the transposition engine must shift the 3×3 grid's root cleanly
  (see `Chord.SetFundamental`, `Drone.GetClosestHarmonizedPitchToOriginalSample`).
- **Respect the Java port:** these classes are a faithful migration. Prefer small, idiomatic C# adjustments
  over redesigns unless asked; when you do refactor, preserve serialization compatibility.
- **Keep this file current:** update the §2 status table and §8 roadmap as things get implemented — it is the
  fast-start context for any future session.
