using System.Windows.Forms;

namespace VirtuoPhone.Config;

/// <summary>
/// Remappable bindings. Every logical action can be driven by a gamepad button (winmm 1-based number, 0 =
/// unbound) AND/OR a keyboard key (<see cref="Keys.None"/> = unbound) — either source triggers it, so gamepad
/// and keyboard are interchangeable. The stick direction also comes from the gamepad axes; the four Dir*Key
/// bindings let the keyboard drive direction too. There are <see cref="NoteCount"/> note-button slots in
/// ascending pitch; the F500's 8 buttons take the lowest 8, the rest are keyboard-only.
/// </summary>
public sealed class ControllerMap
{
    // 48 note-button slots (ascending pitch). The F500's 8 physical buttons take the lowest 8; 0 = unbound.
    public int[] NoteButtons { get; set; } =
    {
        1, 2, 10, 9, 4, 5, 8, 7,                          // 0-7   F500 buttons
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,   // 8-23
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,   // 24-39
        0, 0, 0, 0, 0, 0, 0, 0,                           // 40-47
    };
    public int StartButton { get; set; } = 12;
    public int SelectButton { get; set; } = 11;
    public int HomeButton { get; set; } = 13;

    // 48 keys across the four QWERTY rows, low → high (bottom row → number row).
    public Keys[] NoteKeys { get; set; } =
    {
        Keys.Z, Keys.X, Keys.C, Keys.V, Keys.B, Keys.N, Keys.M, Keys.Oemcomma, Keys.OemPeriod, Keys.OemQuestion,        // 0-9   bottom
        Keys.A, Keys.S, Keys.D, Keys.F, Keys.G, Keys.H, Keys.J, Keys.K, Keys.L, Keys.OemSemicolon, Keys.OemQuotes,      // 10-20 home
        Keys.Q, Keys.W, Keys.E, Keys.R, Keys.T, Keys.Y, Keys.U, Keys.I, Keys.O, Keys.P, Keys.OemOpenBrackets, Keys.OemCloseBrackets,  // 21-32 top
        Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0,                        // 33-42 number
        Keys.OemMinus, Keys.Oemplus, Keys.Oemtilde, Keys.Back, Keys.OemPipe,                                             // 43-47 number-row extras
    };
    public Keys StartKey { get; set; } = Keys.Enter;
    public Keys SelectKey { get; set; } = Keys.Tab;
    public Keys HomeKey { get; set; } = Keys.Escape;

    public Keys UpKey { get; set; } = Keys.Up;
    public Keys DownKey { get; set; } = Keys.Down;
    public Keys LeftKey { get; set; } = Keys.Left;
    public Keys RightKey { get; set; } = Keys.Right;

    public const int NoteCount = 48;

    public ControllerMap Clone() => new()
    {
        NoteButtons = (int[])NoteButtons.Clone(),
        StartButton = StartButton, SelectButton = SelectButton, HomeButton = HomeButton,
        NoteKeys = (Keys[])NoteKeys.Clone(),
        StartKey = StartKey, SelectKey = SelectKey, HomeKey = HomeKey,
        UpKey = UpKey, DownKey = DownKey, LeftKey = LeftKey, RightKey = RightKey,
    };

    /// <summary>Repair a hand-edited / older map so the note arrays are always <see cref="NoteCount"/> long,
    /// filling any newly-added slots from the defaults (so growing an 8-button config to 21 stays usable).</summary>
    public void Normalize()
    {
        var def = new ControllerMap();
        if (NoteButtons.Length != NoteCount) NoteButtons = Extend(NoteButtons, def.NoteButtons);
        if (NoteKeys.Length != NoteCount) NoteKeys = Extend(NoteKeys, def.NoteKeys);
    }

    private static T[] Extend<T>(T[] current, T[] defaults)
    {
        int old = current.Length;
        var a = current;
        Array.Resize(ref a, NoteCount);
        for (int i = old; i < NoteCount; i++) a[i] = defaults[i];
        return a;
    }
}
