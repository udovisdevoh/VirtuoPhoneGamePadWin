using System;

namespace VirtuoPhone.Input;

/// <summary>
/// Maps a winmm joystick (default: the Mayflash F500 on the first connected slot) to logical input.
///
/// The F500's button numbers were captured live (see the probe in Program): note buttons in pitch order are
/// winmm buttons 1, 2, 10, 9, 4, 5, 8, 7; Start=12, Select=11, Home=13; the lever reports on the X/Y axes
/// (POV unused). This mapping is currently hard-coded — the planned remap menu (CLAUDE.md) will replace it.
/// </summary>
public sealed class WinmmControllerInput : IControllerInput
{
    // winmm button numbers (1-based) in note-pitch order, from the live capture on the Mayflash F500.
    private static readonly int[] NoteButtons = { 1, 2, 10, 9, 4, 5, 8, 7 };
    private const int StartButton = 12, SelectButton = 11, HomeButton = 13;
    private const float AxisThreshold = 0.5f;

    private readonly int deviceId;

    public string Name { get; }
    public bool Connected => WinmmJoystick.TryPoll(deviceId, out _);
    public int NoteButtonCount => NoteButtons.Length;

    public WinmmControllerInput(int deviceId)
    {
        this.deviceId = deviceId;
        Name = WinmmJoystick.TryGetCaps(deviceId, out var c) ? c.Name : $"joystick {deviceId}";
    }

    /// <summary>First connected joystick slot, or -1 if none is plugged in.</summary>
    public static int FindFirstConnected()
    {
        for (int id = 0; id < WinmmJoystick.MaxDevices; id++)
            if (WinmmJoystick.TryPoll(id, out _)) return id;
        return -1;
    }

    public InputSnapshot Poll()
    {
        if (!WinmmJoystick.TryPoll(deviceId, out var s))
            return new InputSnapshot(0, Direction.Neutral, false, false, false);

        int mask = 0;
        for (int i = 0; i < NoteButtons.Length; i++)
            if (IsDown(s.Buttons, NoteButtons[i])) mask |= 1 << i;

        return new InputSnapshot(
            mask,
            ToDirection(s.X, s.Y),
            IsDown(s.Buttons, StartButton),
            IsDown(s.Buttons, SelectButton),
            IsDown(s.Buttons, HomeButton));
    }

    private static bool IsDown(uint buttons, int oneBasedButton) => (buttons & (1u << (oneBasedButton - 1))) != 0;

    private static Direction ToDirection(float x, float y)
    {
        // winmm axes: up/left = -1, down/right = +1.
        bool up = y < -AxisThreshold, down = y > AxisThreshold;
        bool left = x < -AxisThreshold, right = x > AxisThreshold;
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
