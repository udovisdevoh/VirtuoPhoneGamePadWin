# Project Context: Arcade Stick Music Synthesizer

## 1. Project Overview
This project is a Windows application (C# Console or Windows Forms) that transforms a video game controller—specifically optimized for arcade fight sticks like the Mayflash F500 Elite—into a polyphonic musical instrument. The software maps 8 action buttons to musical notes within a scale or chord, while the joystick dynamically controls the active chord/scale progression using a 3x3 matrix layout.

## 2. Tech Stack & Architecture
* **Language:** C# (.NET)
* **UI Framework:** It is currently Windows Console but it must be converted to something with UI.
* **Configuration:** JSON for hardware mapping and musical presets.
* **Audio Engine:** Custom low-latency C# implementation modeled after the Android `SoundPool` API (migrated from the reference Java project).
* **Reference Path for Audio Engine Porting:** `..\..\Java\VirtuoPhone\src\com\virtuophone`

## 3. Input Handling & Hardware Mechanics
The input system must be highly responsive to accommodate crisp execution, rapid directional inputs, and low-latency polling.

* **Action Buttons (Right Side):** Up to 8 buttons triggering notes of the currently selected scale/chord. Supports 8-voice polyphony.
* **Joystick (Left Side):** Controls the tonal center and active preset via a 3x3 grid.
    * **Neutral Position:** The main root chord or scale (e.g., E Major).
    * **8 Directions:** Instantly shifts the active chord/scale based on the loaded preset.
* **Dynamic Pitch-Bending / Sample Swapping:** If a button is held down while the joystick changes position, the active note will dynamically pitch-bend or swap to the corresponding sample in the new chord/scale.
* **Directional Double-Tap ("Dash" Mechanic):** Rapidly tapping a cardinal direction twice (Up, Down, Left, Right) triggers an alternate/secondary preset tied to that extremity.
* **System Buttons:**
    * **Select Button:** Toggles through different loaded instruments (sample banks).
    * **Start Button (Modulation):** * *Single Press + Direction:* Transposes the entire preset layout so that the selected direction becomes the new Neutral/Center root. Remaining positions scale relatively.
        * *Double Press:* Forces the new Neutral position to be a Minor chord/scale. Remaining positions follow their predefined explicit values or default to Major.

## 4. Configuration & Presets (JSON)
Presets define the 3x3 musical matrices. The application relies on human-readable JSON files that allow users to add, modify, and select custom layouts. The controller mapping will also be strictly defined in these files.

### Preset Matrix Logic & Examples
Matrices are conceptualized as a 3x3 grid centered around the neutral joystick position, with optional outer extensions for double-tap mechanics.


**Example 0: E pentatonic minor, stable corners, Try this one first, center E is tonic (Double-Tap Outer Bounds)**
| B  | Bb | A  |
| C  | E  | F  |
| D  | F# | G  |



**Example 1: Extended Layout (Double-Tap Outer Bounds)**
           | B+ |
       | D | F# | G |
|F#dim7| F | E  | B |C#dim7|
       | C | Bb | A |
           | E+ |

*(Center: E, Diagonals: F#, G, A, Bb. Outer edges require double-tap inputs).*

**Example 2: Chromatic Spiral**
| C  | D  | F  |
| B  | E  | F# |
| Bb | A  | G  |


**Example 3: Chromatic Spiral (Circle of 5ths)**
| F# | B  | A  |
| Bb | E  | D  |
| F  | C  | G  |


**Example 4: Minor Blues Rotation (Tensions on sides, Stable on diagonals)**
| B  | C  | D  |
| Bb | E  | F# |
| A  | F  | G  |


**Example 5: Minor Blues Rotation (Tensions on diagonals, Stable on sides)**
| C  | D  | F# |
| B  | E  | G  |
| Bb | A  | F  |
(Underlying set: F# B E A D G C F Bb)


**Example 6: Pure Circle of 4ths Rotation**
| C  | G  | D  |
| F  | E  | A  |
| Bb | Eb | Ab |


**Example 7: Center dim7 (Most unstable center, stable corners)**
| C  | Bb | D# |
| G  | Bo | C# |
| A  | E  | F# |




## 5. Development Directives for the AI
* **Prioritize Low Latency:** When generating C# audio code, prioritize architectures that mimic Android's `SoundPool`. Focus on pre-loading buffers and ensuring real-time polyphonic playback without audio dropouts.
* **Input State Management:** Ensure the input loop accurately distinguishes between held notes, single direction changes, and the double-tap timing window without blocking the audio thread.
* **Relative Math for Modulation:** The transposition engine must elegantly handle interval math to accurately shift the 3x3 grid's root note when the Start button logic is triggered.
*** This document will give Claude (or any LLM) immediate, structured context regarding your input mechanics, your specific transposition logic, and the exact path of the Java legacy code it needs to reference. Let me know if you are ready to move on to the next phase of the project!
* Improve this documentation while working on the project