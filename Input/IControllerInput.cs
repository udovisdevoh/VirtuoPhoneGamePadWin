namespace VirtuoPhone.Input;

/// <summary>The 3×3 joystick grid cell: neutral center + 8 compass directions.</summary>
public enum Direction { Neutral, Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft }

/// <summary>
/// One immutable input reading. Note buttons are a bitmask (bit <c>i</c> = note button <c>i</c> held),
/// so a snapshot is a value type with no allocation — cheap to poll every tick and diff.
/// </summary>
public readonly record struct InputSnapshot(long NotesMask, Direction Dir, bool Start, bool Select, bool Home)
{
    public bool Note(int i) => (NotesMask & (1L << i)) != 0;
}

/// <summary>
/// A logical controller: unifies gamepad and keyboard behind note buttons + a 3×3 direction + system
/// buttons. Poll it each tick and diff snapshots for note-on/off and chord-cell changes. Mapping/remapping
/// lives in the implementation (later: JSON-backed remap; a keyboard implementation is a second source).
/// </summary>
public interface IControllerInput
{
    string Name { get; }
    bool Connected { get; }
    int NoteButtonCount { get; }
    InputSnapshot Poll();
}
