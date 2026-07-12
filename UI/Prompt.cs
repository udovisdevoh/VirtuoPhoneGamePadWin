using System.Drawing;
using System.Windows.Forms;

namespace VirtuoPhone.UI;

/// <summary>Minimal modal text-input dialog (WinForms has no built-in InputBox).</summary>
public static class Prompt
{
    public static string? Text(IWin32Window owner, string title, string label, string initial = "")
    {
        using var form = new Form
        {
            Text = title, FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false, MaximizeBox = false, ClientSize = new Size(360, 110), Font = new Font("Segoe UI", 9f),
        };
        var lbl = new Label { Text = label, Left = 12, Top = 12, AutoSize = true };
        var box = new TextBox { Text = initial, Left = 12, Top = 36, Width = 336 };
        var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 192, Top = 70, Width = 75 };
        var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 273, Top = 70, Width = 75 };
        form.Controls.AddRange(new Control[] { lbl, box, ok, cancel });
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        return form.ShowDialog(owner) == DialogResult.OK && box.Text.Trim().Length > 0 ? box.Text.Trim() : null;
    }
}
