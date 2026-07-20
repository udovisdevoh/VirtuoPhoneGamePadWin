using VirtuoPhone.Config;

namespace VirtuoPhone.Input;

/// <summary>
/// An <see cref="IControllerInput"/> that reads a winmm gamepad AND the keyboard, resolving both through a
/// remappable <see cref="ControllerMap"/>. Either source triggers an action, so gamepad and keyboard are
/// interchangeable; with no gamepad (<c>deviceId &lt; 0</c>) it is keyboard-only. The map can be swapped live
/// (from the remap UI) via <see cref="SetMap"/>.
/// </summary>
public sealed class MappedControllerInput : IControllerInput
{
    private const float AxisThreshold = 0.5f;

    private readonly int deviceId;          // winmm slot, or -1 for keyboard only
    private volatile ControllerMap map;
    private Direction latchedKeyDir = Direction.Neutral;   // keyboard-selected chord cell (persists on release)

    public string Name { get; }
    public bool Connected => true;          // the keyboard is always available
    public int NoteButtonCount => ControllerMap.NoteCount;

    public MappedControllerInput(int deviceId, ControllerMap map)
    {
        this.deviceId = deviceId;
        this.map = map;
        Name = deviceId >= 0 && WinmmJoystick.TryGetCaps(deviceId, out var c) ? $"{c.Name} + keyboard" : "Keyboard only";
    }

    public void SetMap(ControllerMap newMap) => map = newMap;

    public InputSnapshot Poll()
    {
        ControllerMap m = map;

        uint buttons = 0;
        float x = 0, y = 0;
        bool hasPad = false;
        if (deviceId >= 0 && WinmmJoystick.TryPoll(deviceId, out var s))
        {
            hasPad = true;
            buttons = s.Buttons; x = s.X; y = s.Y;
        }

        long mask = 0;
        for (int i = 0; i < ControllerMap.NoteCount; i++)
            if (PadDown(buttons, m.NoteButtons[i]) || KeyboardReader.IsDown(m.NoteKeys[i])) mask |= 1L << i;

        // Stick direction is MOMENTARY (reverts to neutral on release).
        Direction stick = ToDirection(hasPad && y < -AxisThreshold, hasPad && y > AxisThreshold,
                                      hasPad && x < -AxisThreshold, hasPad && x > AxisThreshold);

        // Keyboard direction LATCHES: it follows the keys while held and persists the last non-neutral value
        // after release; the neutral key clears it. (The keyboard can't comfortably hold a direction while the
        // other hand plays notes, so the chord/scale selection stays put until you pick another or hit neutral.)
        Direction liveKeys;
        if (KeyboardReader.IsDown(m.UpLeftKey)) liveKeys = Direction.UpLeft;            // dedicated diagonal keys
        else if (KeyboardReader.IsDown(m.UpRightKey)) liveKeys = Direction.UpRight;
        else if (KeyboardReader.IsDown(m.DownLeftKey)) liveKeys = Direction.DownLeft;
        else if (KeyboardReader.IsDown(m.DownRightKey)) liveKeys = Direction.DownRight;
        else liveKeys = ToDirection(KeyboardReader.IsDown(m.UpKey), KeyboardReader.IsDown(m.DownKey),   // else combine cardinals
                                    KeyboardReader.IsDown(m.LeftKey), KeyboardReader.IsDown(m.RightKey));
        if (KeyboardReader.IsDown(m.NeutralKey)) latchedKeyDir = Direction.Neutral;
        else if (liveKeys != Direction.Neutral) latchedKeyDir = liveKeys;

        // The stick stays MOMENTARY exactly as before: while it's pushed it takes over AND clears any keyboard
        // latch, so releasing the stick always returns to neutral — the joystick never leaves a chord stuck.
        if (stick != Direction.Neutral) latchedKeyDir = Direction.Neutral;

        Direction dir = stick != Direction.Neutral ? stick : latchedKeyDir;

        bool start  = PadDown(buttons, m.StartButton)  || KeyboardReader.IsDown(m.StartKey);
        bool select = PadDown(buttons, m.SelectButton) || KeyboardReader.IsDown(m.SelectKey);
        bool home   = PadDown(buttons, m.HomeButton)   || KeyboardReader.IsDown(m.HomeKey);

        return new InputSnapshot(mask, dir, start, select, home);
    }

    private static bool PadDown(uint buttons, int oneBasedButton) =>
        oneBasedButton > 0 && (buttons & (1u << (oneBasedButton - 1))) != 0;

    private static Direction ToDirection(bool up, bool down, bool left, bool right)
    {
        if (up && left) return Direction.UpLeft;
        if (up && right) return Direction.UpRight;
        if (down && left) return Direction.DownLeft;
        if (down && right) return Direction.DownRight;
        if (up) return Direction.Up;
        if (down) return Direction.Down;
        if (left) return Direction.Left;
        if (right) return Direction.Right;
        return Direction.Neutral;
    }
}
