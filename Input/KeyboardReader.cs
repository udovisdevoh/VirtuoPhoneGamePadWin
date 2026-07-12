using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VirtuoPhone.Input;

/// <summary>Polls physical key state via Win32 <c>GetAsyncKeyState</c> — works regardless of window focus and
/// fits the engine's poll loop (no event queue). <see cref="Keys"/> values are the virtual-key codes.</summary>
internal static class KeyboardReader
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    public static bool IsDown(Keys key) => key != Keys.None && (GetAsyncKeyState((int)key) & 0x8000) != 0;
}
