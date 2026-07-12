using System.Drawing;
using System.Windows.Forms;
using VirtuoPhone.Input;

namespace VirtuoPhone.UI;

/// <summary>Modal "press a key" / "press a controller button" capture dialogs for the remap tab.
/// Returns null when cancelled; a real value (including the "unbind" sentinel) means the binding changed.</summary>
public static class InputCapture
{
    /// <summary>Capture one keypress. Returns the key, <see cref="Keys.None"/> to unbind, or null if cancelled.</summary>
    public static Keys? Key(IWin32Window owner)
    {
        using var form = MakeDialog("Press a key…", out var panel);
        Keys captured = Keys.None;
        bool got = false;

        form.KeyPreview = true;
        form.KeyDown += (_, e) =>
        {
            e.Handled = e.SuppressKeyPress = true;      // intercept Tab/Enter/Space/etc. before controls see them
            captured = e.KeyCode;
            got = true;
            form.DialogResult = DialogResult.OK;
        };
        panel.Controls.Add(MakeButton("Unbind", () => { captured = Keys.None; got = true; form.DialogResult = DialogResult.OK; }));
        panel.Controls.Add(MakeButton("Cancel", () => form.DialogResult = DialogResult.Cancel));

        return form.ShowDialog(owner) == DialogResult.OK && got ? captured : null;
    }

    /// <summary>Capture one gamepad button. Returns the winmm 1-based number, 0 to unbind, or null if cancelled.</summary>
    public static int? Button(IWin32Window owner, int deviceId)
    {
        if (deviceId < 0)
        {
            MessageBox.Show(owner, "No controller connected — keyboard only.");
            return null;
        }

        using var form = MakeDialog("Press a controller button…", out var panel);
        int captured = 0;
        bool got = false;

        uint initial = WinmmJoystick.TryPoll(deviceId, out var s0) ? s0.Buttons : 0;
        var timer = new System.Windows.Forms.Timer { Interval = 25 };
        timer.Tick += (_, _) =>
        {
            if (!WinmmJoystick.TryPoll(deviceId, out var s)) return;
            uint newly = s.Buttons & ~initial;
            if (newly == 0) return;
            int bit = 0;
            while ((newly & (1u << bit)) == 0) bit++;
            captured = bit + 1;
            got = true;
            form.DialogResult = DialogResult.OK;
        };
        panel.Controls.Add(MakeButton("Unbind", () => { captured = 0; got = true; form.DialogResult = DialogResult.OK; }));
        panel.Controls.Add(MakeButton("Cancel", () => form.DialogResult = DialogResult.Cancel));

        timer.Start();
        var result = form.ShowDialog(owner);
        timer.Stop();
        return result == DialogResult.OK && got ? captured : null;
    }

    private static Form MakeDialog(string prompt, out FlowLayoutPanel buttons)
    {
        var form = new Form
        {
            Text = "Rebind", FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false, MaximizeBox = false, ClientSize = new Size(320, 110), Font = new Font("Segoe UI", 9f),
        };
        form.Controls.Add(new Label { Text = prompt, Left = 14, Top = 18, AutoSize = true, Font = new Font("Segoe UI", 10f) });
        buttons = new FlowLayoutPanel { Left = 14, Top = 60, Width = 292, Height = 34, FlowDirection = FlowDirection.RightToLeft };
        form.Controls.Add(buttons);
        return form;
    }

    private static Button MakeButton(string text, Action onClick)
    {
        var b = new Button { Text = text, AutoSize = true };
        b.Click += (_, _) => onClick();
        return b;
    }
}
