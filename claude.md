# Project Context: Arcade Stick Music Synthesizer (VirtuoPhone GamePad — Windows)

## 1. Project Overview
A Windows app (C#/.NET 8) that turns a video-game controller — tuned for arcade fight sticks
like the **Mayflash F500 Elite** — into a polyphonic musical instrument.

- **8 action buttons (right)** → play the notes of the currently selected scale/chord (8-voice polyphony).
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
| Low-latency audio engine (SoundPool-equivalent) | ✅ **Working (first cut)** | `Audio/NAudioSoundPool.cs` — WASAPI shared (~50 ms), polyphonic, per-voice pitch; behind an `ISoundPool` seam. `DummySoundPool` kept as a silent/headless backend |
| **Project compiles & plays audio** | ✅ **Yes** | `R` class generated; NAudio + NAudio.Vorbis added; smoke test plays a polyphonic arpeggio (verified) |
| Controller / gamepad input | ❌ Not started | No XInput/DirectInput, no F500 mapping |
| UI | ❌ Not started | `Program.cs` is still the `Hello, World!` template |
| JSON presets & remappable controls | ❌ Not started | Presets currently exist only as hard-coded text-serialized strings (§5), **not** JSON yet |
| Modulation / double-tap / pitch-bend-on-move logic | ❌ Not started | Fully specified in §6, unimplemented |

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
- **Language / runtime:** C# on **.NET 8.0** (`net8.0`, `ImplicitUsings=enable`, `Nullable=enable`).
- **Output:** Console `Exe` today; a real UI comes later (framework deferred — keep the core headless/decoupled;
  when built, favor simple-to-maintain + performant).
- **Configuration:** target is human-readable **JSON** for hardware mapping + musical presets (not built yet).
- **Audio Engine:** custom low-latency C# engine modeled on Android `SoundPool` (pre-loaded buffers,
  real-time polyphonic playback, per-stream rate/volume for pitch-shift). Currently faked by `DummySoundPool`.
- **Namespaces:** root `VirtuoPhone` (`AppController`, `DummySoundPool`, `PointerMemory`) and
  `VirtuoPhone.Models` (everything under `Models/`). Note: files in `Audio/` use the root `VirtuoPhone` namespace.

### Decisions (2026-07-10)
- **Approach:** build a **vertical slice first** — one instrument + one hard-coded preset + the real F500 → sound —
  and measure end-to-end latency before adding breadth.
- **Audio (built):** **`NAudioSoundPool`** = NAudio + NVorbis, behind **`ISoundPool`** (swappable via
  `AudioBackend`). Source samples are **mono 44.1 kHz**. The engine runs at the **output/device rate** and
  each sample is **resampled once, in RAM, at load** (cubic) to that rate — so the **playback path never
  resamples** (only genuine pitch shifts interpolate, **cubic / Catmull-Rom**) and, in shared mode, the OS
  doesn't resample the mix every buffer. An unshifted note then plays at step 1.0.
  **Output defaults to WASAPI _shared_ at the device mix rate** so the **Windows volume slider works**,
  attacks aren't affected by exclusive stream start/stop, and other apps' audio coexists. Set
  **`VP_AUDIO_MODE=exclusive`** for **44.1 kHz bit-exact**, lowest latency — but it **bypasses the Windows
  volume mixer** and takes the device exclusively. Mix bus = per-poly headroom + a **soft-knee** limiter (no
  hard clipping). **Do not resample/filter on the _playback_ path, and do not add attack/release envelopes**
  (they smear the pluck transient — the user rejected both). Resampling **once at load** is the sanctioned
  way to match the device rate.
- **Audio real-time rules (learned the hard way):** the mixer runs on the WASAPI render thread — keep it
  **allocation-free** (fixed voice-slot pool, no per-buffer enumerator) or the GC pauses it into underruns.
  Low-latency "pops" were **buffer underruns**, not signal bugs: **5 ms underran** with NAudio's default
  render thread, **10 ms is clean and punchy**. To go lower reliably, add a custom WASAPI render thread
  registered with **MMCSS "Pro Audio"**. The `VP_LATENCY_MS` env var forces a buffer size for testing.
- **Input:** **DirectInput/HID** (or a standard Windows joystick), behind an **`IControllerInput`** abstraction
  that treats **gamepad and keyboard uniformly**. A **remap menu** binds each musical/system action to either a
  controller button/axis *or* a keyboard key. Keep it open to other backends.
  *(Dev/test device: a Mayflash F500 Elite is connected via USB and available for live input capture.)*
- **UI:** deferred; core stays headless and decoupled so any UI (WinForms/WPF/Avalonia) can bind later.
- **Resource ids (`R.Raw.*`):** resolved with an **auto-generated `R` class** (int ids → `res/raw/<name>.ogg`),
  generated from the union of code references and bundled files; regenerate when samples change.

---

## 4. Codebase Map
```
VirtuoPhoneGamePadWin.csproj   .NET 8 console exe
Program.cs                     Entry point — still "Hello, World!" stub (needs app wiring)
AppController.cs               Singleton; STRING_COUNT=6; GetInstrument() hard-codes new SteelGuitar()
Audio/
  ISoundPool.cs                SoundPool API surface — the swappable audio seam
  NAudioSoundPool.cs           Real engine: NVorbis decode, WASAPI exclusive @44.1k, cubic voice mixer + soft-knee
  DummySoundPool.cs            Silent/headless ISoundPool (tests / no audio device)
  AudioBackend.cs              Factory selecting the ISoundPool backend (default: NAudio)
  PointerMemory.cs             Tracks active stream IDs so they can be stopped together
Models/
  Note.cs                      Pitch as semitone int; C..B constants; pitch = noteType + octave*12
  Chord.cs                     Hard-coded voicings per ChordType; transpose; (de)serialize "name:typeId:noteType"
  ChordType.cs                 enum: maj/min/7ths/9ths/sus/dim/aug/pentatonics/"ff_" full-fret sets…
  GuitarPreset.cs              Ordered List<Chord>; indexed by Point(X=string, Y=chord); serialize by lines
  Sample.cs                    One .ogg mapped to an original pitch; 12-TET rate = 1.0594632^(Δsemitone+bend)
  MultiSampleSet.cs            Multiple samples for one pitch; GetRandomSample() for round-robin variation
  StringExpander.cs            Grow/shrink a chord's note count to match instrument string count
  Drone.cs                     Sustained bass that follows chord roots (glide, lazy-harmonic, min-pitch-shift)
  PlayingStringMemory.cs       Per-string → stream-id map (mute previous note on same string)
  Instruments/
    Instrument.cs              Abstract base: preloads samples into multiSampleList[128], interpolates gaps,
                               Play/Stop/SetStreamPitch, feature flags via abstract Build*() methods
    Bagpipes, Harp, Harpsichord, JewsHarp, Piano, Sitar, SteelGuitar, Synth, Violin  (9 concrete instruments)
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
  - **`stringCount` = the number of note buttons = 8 for every instrument** (like a piano's 8-note layout);
    chords are voiced / `StringExpander`-ed to 8 notes. The old per-instrument counts (6, 4, 17…) were an
    Android-app vestige and have been unified to 8 — always use 8.
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

- **Action Buttons (right, up to 8):** trigger notes of the active scale/chord; 8-voice polyphony.
- **Joystick (left):** selects tonal center / active preset cell via a 3×3 grid.
  - **Neutral:** the root chord/scale (e.g. E).
  - **8 directions:** instantly shift the active chord/scale per the loaded preset.
- **Dynamic pitch-bend / sample-swap:** if a button is **held** while the joystick moves, the sounding note
  pitch-bends or swaps to the corresponding sample in the new chord/scale (see `Instrument.SetStreamPitch`,
  `IsPitchBend`).
- **Directional double-tap ("dash"):** quickly tapping a cardinal direction twice triggers an alternate
  preset tied to that extremity (the outer cells in the extended matrices).
- **System buttons:**
  - **Select:** cycle loaded instruments (sample banks).
  - **Start (modulation):**
    - *Single + direction:* transpose the whole layout so that direction becomes the new neutral/center root;
      other cells scale relatively (interval math on the 3×3 grid).
    - *Double press:* force the new neutral cell to be **minor**; other cells keep their explicit preset value
      or default to major.
- **Remappable controls:** a **remap menu** lets every action be rebound to a controller button/axis **or a
  keyboard key** (keyboard and gamepad are interchangeable input sources), persisted in the JSON config.

### Voice management & note lifecycle (implemented in the play harness `Program.cs`)
How held buttons + joystick chord changes map onto engine voices:
- **Per-button polyphony.** ✅ Each action button owns its own voice; re-pressing/swapping steals *that
  button's* voice (via the instrument's same-string mute). Polyphony is per button, not global.
- **Chord change while holding.** ✅ When the joystick changes the chord, each held button whose note changed
  is re-voiced immediately (no re-press); unchanged notes keep ringing.
- **Per-instrument note-off.** ✅ On release, **looping/sustained** instruments (`IsAutoLoop()`: violin,
  bagpipes) **stop**; **plucked** ones (guitar, piano, harp, harpsichord, sitar, jew's-harp, synth) ring out.
- **Monophonic instruments** (`IsMonophonic()`: bagpipes chanter). ✅ **Last-note priority with fall-back**:
  a new press replaces the sounding note; releasing it falls back to the newest still-held button (a held
  stack in the harness). The drone is separate, so bagpipes is effectively "bi-phonic".
- **Optional per-voice envelope.** ✅ `Instrument.BuildAttackSeconds()`/`BuildReleaseSeconds()` (default 0 =
  hard edge) drive a per-voice attack/release fade in `NAudioSoundPool`. Only the **violin** sets them > 0.
  This is **opt-in per instrument** — do not apply an envelope globally (an always-on envelope was rejected).
- **Drone instruments.** ✅ **sitar** (C# tampura) and **bagpipes** (A) add a sustained root that tracks the
  chord (the `Drone` class via `LoadDrone`/`setDrone`; lazy-harmonic + a `pitchAdjustSpeedMultiplicator`
  glide — bagpipes uses a small **1.02** so its small per-chord steps glide instead of snapping). Harp,
  harpsichord, steel guitar have no drone. A **`BuildIsDroneFixed()`** flag exists (default false, currently
  **unused/dormant**) to hold a drone fixed against joystick chords — kept for a future instrument.

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
2. ✅ **Audio engine** — `NAudioSoundPool` (NAudio + NVorbis, WASAPI shared ~50 ms, polyphonic, per-voice
   pitch, tanh mix-bus limiter + voice cap) behind `ISoundPool`; swappable via `AudioBackend`.
   `DummySoundPool` kept as the headless backend.
3. ✅ **Prove audio end-to-end** — `Program.cs` plays a chord through the instrument: bit-exact, no
   clipping, no pops at ~10 ms exclusive (verified by ear).
4. ⏳ **Controller input (next):** read the Mayflash F500 (DirectInput/HID) + keyboard behind
   `IControllerInput`, map buttons→notes / joystick→chord cell, driven off the audio thread.
5. **Preset persistence (JSON):** define the JSON schema for presets (3×3 chord layouts) and controller
   maps; implement load and save; migrate the §5 text serialization (or ship a converter).
6. **Preset editing:** let the user build and modify chord layouts — set each joystick cell's chord
   (root + `ChordType`), and rename / add / remove / duplicate / select presets — then save and reload them.
   The model already has the primitives (`GuitarPreset.SetChord/ReplaceChord/RemoveChord/Add/RegenerateChords`,
   `Chord.SetFundamental`); this adds the authoring workflow (its editor UI lands with the UI work below).
7. **Interaction logic & voice management:** ✅ done in the play harness — per-button polyphony, held-note
   swap on chord change, per-instrument note-off, monophony (last-note priority + fall-back), optional
   violin envelope, drones (bagpipes/sitar), and Select = change instrument. **Remaining:** the double-tap
   "dash" alternate presets and the **Start-button modulation** (transpose / force-minor). See §6.
8. **UI:** replace the console with a real UI showing the active cell, instrument, and note layout,
   and hosting the preset editor from step 6.
9. **Configurable audio (eventually):** expose the engine's currently baked-in choices as user settings
   (in the JSON config) — output **latency**, **exclusive vs shared**, **output device**, and **master
   volume**, plus a lower-latency **MMCSS "Pro Audio"** render path. Defaults stay as today (WASAPI
   exclusive ~10 ms, 44.1 kHz); the `VP_LATENCY_MS` env var is only a temporary test hook to be replaced.
10. **Voice leading (later):** when the joystick changes chord, pick the voicing that is the **closest
    inversion** to the neutral/center chord — minimising how far each button's note moves. E.g. centre E
    major `E B E G# B E G# B` → C major becomes `E C E G C E G C` (nearest inversion; notes move 0/±1).
    Build a **dedicated class** for this (inspired by `StringExpander` / `Chord` in the ported code). It must
    also fix **chord-tone purity**: the ported `Chord` voicing templates contain non-chord tones — e.g. the
    `maj` template is `E A E G# B E` (an **A**, the 4th) when E major should be `E B E G# B E`. Voicings must
    use **only the chord's tones**, generated from the chord rather than the ported guitar templates.

---

## 9. Build & Run
```powershell
dotnet build      # currently FAILS until the R class exists (see §2)
dotnet run        # runs Program.cs (Hello World stub today)
```
Restore/build target framework is `net8.0`. When adding audio/input NuGet packages, add them to
`VirtuoPhoneGamePadWin.csproj`.

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
