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

        int mask = 0;
        for (int i = 0; i < ControllerMap.NoteCount; i++)
            if (PadDown(buttons, m.NoteButtons[i]) || KeyboardReader.IsDown(m.NoteKeys[i])) mask |= 1 << i;

        bool up    = (hasPad && y < -AxisThreshold) || KeyboardReader.IsDown(m.UpKey);
        bool down  = (hasPad && y >  AxisThreshold) || KeyboardReader.IsDown(m.DownKey);
        bool left  = (hasPad && x < -AxisThreshold) || KeyboardReader.IsDown(m.LeftKey);
        bool right = (hasPad && x >  AxisThreshold) || KeyboardReader.IsDown(m.RightKey);

        bool start  = PadDown(buttons, m.StartButton)  || KeyboardReader.IsDown(m.StartKey);
        bool select = PadDown(buttons, m.SelectButton) || KeyboardReader.IsDown(m.SelectKey);
        bool home   = PadDown(buttons, m.HomeButton)   || KeyboardReader.IsDown(m.HomeKey);

        return new InputSnapshot(mask, ToDirection(up, down, left, right), start, select, home);
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
