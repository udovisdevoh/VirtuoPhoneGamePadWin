using System.Windows.Forms;

namespace VirtuoPhone.Config;

/// <summary>
/// Remappable bindings. Every logical action can be driven by a gamepad button (winmm 1-based number, 0 =
/// unbound) AND/OR a keyboard key (<see cref="Keys.None"/> = unbound) — either source triggers it, so gamepad
/// and keyboard are interchangeable. The stick direction also comes from the gamepad axes; the four Dir*Key
/// bindings let the keyboard drive direction too. Defaults match the Mayflash F500 plus a WASD-ish layout.
/// </summary>
public sealed class ControllerMap
{
    public int[] NoteButtons { get; set; } = { 1, 2, 10, 9, 4, 5, 8, 7 };   // F500 note buttons in pitch order
    public int StartButton { get; set; } = 12;
    public int SelectButton { get; set; } = 11;
    public int HomeButton { get; set; } = 13;

    public Keys[] NoteKeys { get; set; } =
        { Keys.A, Keys.S, Keys.D, Keys.F, Keys.G, Keys.H, Keys.J, Keys.K };
    public Keys StartKey { get; set; } = Keys.Enter;
    public Keys SelectKey { get; set; } = Keys.Tab;
    public Keys HomeKey { get; set; } = Keys.Escape;

    public Keys UpKey { get; set; } = Keys.Up;
    public Keys DownKey { get; set; } = Keys.Down;
    public Keys LeftKey { get; set; } = Keys.Left;
    public Keys RightKey { get; set; } = Keys.Right;

    public const int NoteCount = 8;

    public ControllerMap Clone() => new()
    {
        NoteButtons = (int[])NoteButtons.Clone(),
        StartButton = StartButton, SelectButton = SelectButton, HomeButton = HomeButton,
        NoteKeys = (Keys[])NoteKeys.Clone(),
        StartKey = StartKey, SelectKey = SelectKey, HomeKey = HomeKey,
        UpKey = UpKey, DownKey = DownKey, LeftKey = LeftKey, RightKey = RightKey,
    };

    /// <summary>Repair a hand-edited / older map so the arrays are always the right length.</summary>
    public void Normalize()
    {
        if (NoteButtons.Length != NoteCount) { var a = NoteButtons; Array.Resize(ref a, NoteCount); NoteButtons = a; }
        if (NoteKeys.Length != NoteCount) { var k = NoteKeys; Array.Resize(ref k, NoteCount); NoteKeys = k; }
    }
}
