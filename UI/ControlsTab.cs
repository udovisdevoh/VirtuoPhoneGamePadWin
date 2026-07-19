using System.Drawing;
using System.Windows.Forms;
using VirtuoPhone.Config;

namespace VirtuoPhone.UI;

/// <summary>Remap tab: every action can be bound to a gamepad button and/or a keyboard key (either triggers
/// it). Directions come from the stick on the gamepad, or the four keys here. Changes apply live and persist.</summary>
public sealed class ControlsTab : UserControl
{
    private readonly MainForm main;
    private readonly ControllerMap map;
    private TableLayoutPanel grid = null!;

    public ControlsTab(MainForm main)
    {
        this.main = main;
        map = main.Config.Controls;
        Dock = DockStyle.Fill;
        Build();
    }

    private void Build()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(new Label
        {
            AutoSize = true, MaximumSize = new Size(760, 0), Margin = new Padding(0, 0, 0, 8),
            Text = $"Controller: {main.Input.Name}.  Gamepad and keyboard both work — either source triggers an "
                 + "action. Directions use the stick (gamepad) or the four direction keys below.",
        }, 0, 0);

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        grid = new TableLayoutPanel { ColumnCount = 5, AutoSize = true, Dock = DockStyle.Top };
        AddHeader();
        for (int i = 0; i < ControllerMap.NoteCount; i++)
        {
            int idx = i;
            AddRow($"Note {i + 1}", () => map.NoteButtons[idx], v => map.NoteButtons[idx] = v, () => map.NoteKeys[idx], v => map.NoteKeys[idx] = v);
        }
        AddRow("Start (hold = +octave)", () => map.StartButton, v => map.StartButton = v, () => map.StartKey, v => map.StartKey = v);
        AddRow("Select (instrument)", () => map.SelectButton, v => map.SelectButton = v, () => map.SelectKey, v => map.SelectKey = v);
        AddRow("Home (modulate)", () => map.HomeButton, v => map.HomeButton = v, () => map.HomeKey, v => map.HomeKey = v);
        AddRow("Direction Up", null, null, () => map.UpKey, v => map.UpKey = v);
        AddRow("Direction Down", null, null, () => map.DownKey, v => map.DownKey = v);
        AddRow("Direction Left", null, null, () => map.LeftKey, v => map.LeftKey = v);
        AddRow("Direction Right", null, null, () => map.RightKey, v => map.RightKey = v);
        scroll.Controls.Add(grid);
        root.Controls.Add(scroll, 0, 1);

        Controls.Add(root);
    }

    private void AddHeader()
    {
        var bold = new Font("Segoe UI", 9f, FontStyle.Bold);
        string[] cols = { "Action", "Gamepad", "", "Keyboard", "" };
        int row = grid.RowCount++;
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        for (int c = 0; c < cols.Length; c++)
            grid.Controls.Add(new Label { Text = cols[c], AutoSize = true, Font = bold, Margin = new Padding(3, 3, 12, 6) }, c, row);
    }

    private void AddRow(string name, Func<int>? getBtn, Action<int>? setBtn, Func<Keys> getKey, Action<Keys> setKey)
    {
        int row = grid.RowCount++;
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.Controls.Add(new Label { Text = name, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 16, 3) }, 0, row);

        if (getBtn != null && setBtn != null)
        {
            var lblBtn = new Label { Text = BtnText(getBtn()), AutoSize = true, MinimumSize = new Size(64, 0), Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
            grid.Controls.Add(lblBtn, 1, row);
            var re = new Button { Text = "Set button", AutoSize = true, Margin = new Padding(3) };
            re.Click += (_, _) => { var v = InputCapture.Button(this, main.DeviceId); if (v.HasValue) { setBtn(v.Value); lblBtn.Text = BtnText(v.Value); Changed(); } };
            grid.Controls.Add(re, 2, row);
        }
        else
        {
            grid.Controls.Add(new Label { Text = "stick", AutoSize = true, ForeColor = Color.Gray, Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) }, 1, row);
        }

        var lblKey = new Label { Text = KeyText(getKey()), AutoSize = true, MinimumSize = new Size(84, 0), Anchor = AnchorStyles.Left, Margin = new Padding(3, 8, 3, 3) };
        grid.Controls.Add(lblKey, 3, row);
        var reKey = new Button { Text = "Set key", AutoSize = true, Margin = new Padding(3) };
        reKey.Click += (_, _) => { var v = InputCapture.Key(this); if (v.HasValue) { setKey(v.Value); lblKey.Text = KeyText(v.Value); Changed(); } };
        grid.Controls.Add(reKey, 4, row);
    }

    private static string BtnText(int b) => b > 0 ? $"btn {b}" : "—";
    private static string KeyText(Keys k) => k == Keys.None ? "—" : k.ToString();

    private void Changed()
    {
        main.PushControls();
        main.Save();
    }
}
