using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VirtuoPhone.Config;
using VirtuoPhone.Input;
using VirtuoPhone.Models;

namespace VirtuoPhone.UI;

/// <summary>Editor for the preset library: pick a preset, edit each grid cell's chord (root + type), and
/// manage presets (new/duplicate/rename/delete/save). Edits are pushed live to the running engine.</summary>
public sealed class PresetTab : UserControl
{
    private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    // Grid position (column, row) in a 5×5 layout for each cell.
    private static readonly Dictionary<Direction, Point> InnerPos = new()
    {
        [Direction.UpLeft] = new(1, 1), [Direction.Up] = new(2, 1), [Direction.UpRight] = new(3, 1),
        [Direction.Left] = new(1, 2), [Direction.Neutral] = new(2, 2), [Direction.Right] = new(3, 2),
        [Direction.DownLeft] = new(1, 3), [Direction.Down] = new(2, 3), [Direction.DownRight] = new(3, 3),
    };
    private static readonly Dictionary<Direction, Point> OuterPos = new()
    {
        [Direction.Up] = new(2, 0), [Direction.Left] = new(0, 2), [Direction.Right] = new(4, 2), [Direction.Down] = new(2, 4),
    };

    private readonly MainForm main;
    private readonly AppConfig config;
    private readonly Dictionary<Direction, Button> innerBtn = new();
    private readonly Dictionary<Direction, Button> outerBtn = new();

    private ComboBox cboPreset = null!, cboRoot = null!, cboType = null!;
    private Label lblSel = null!;
    private Direction selDir = Direction.Neutral;
    private bool selOuter;
    private bool suppress;

    private Preset Active => config.Presets[config.ActivePreset];

    public PresetTab(MainForm main)
    {
        this.main = main;
        config = main.Config;
        Dock = DockStyle.Fill;
        Build();
        ReloadPresetList();
        RefreshGrid();
        SelectCell(Direction.Neutral, false);
    }

    private void Build()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(10) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        // --- Preset management row ---
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        cboPreset = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
        cboPreset.SelectedIndexChanged += (_, _) =>
        {
            if (suppress) return;
            config.ActivePreset = cboPreset.SelectedIndex;
            RefreshGrid(); SelectCell(Direction.Neutral, false);
            main.PushPreset(Active); main.Save();
        };
        bar.Controls.Add(new Label { Text = "Preset:", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
        bar.Controls.Add(cboPreset);
        bar.Controls.Add(MakeButton("New", NewPreset));
        bar.Controls.Add(MakeButton("Duplicate", DuplicatePreset));
        bar.Controls.Add(MakeButton("Rename", RenamePreset));
        bar.Controls.Add(MakeButton("Delete", DeletePreset));
        root.Controls.Add(bar, 0, 0);   // no Save button — every edit auto-saves to the JSON config

        // --- 5×5 cell grid ---
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 5, Margin = new Padding(0, 8, 0, 8) };
        for (int i = 0; i < 5; i++) { grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20)); grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20)); }
        foreach (var (d, p) in InnerPos) { var b = MakeCellButton(d, false); innerBtn[d] = b; grid.Controls.Add(b, p.X, p.Y); }
        foreach (var (d, p) in OuterPos) { var b = MakeCellButton(d, true); outerBtn[d] = b; grid.Controls.Add(b, p.X, p.Y); }
        root.Controls.Add(grid, 0, 1);

        // --- Selected-cell editor ---
        var edit = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false };
        lblSel = new Label { Text = "Cell:", AutoSize = true, Margin = new Padding(0, 8, 6, 0), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        cboRoot = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
        cboRoot.Items.AddRange(NoteNames);
        cboType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
        cboType.Items.AddRange(Enum.GetValues<ChordType>().Cast<object>().ToArray());
        cboRoot.SelectedIndexChanged += (_, _) => EditSelectedCell();
        cboType.SelectedIndexChanged += (_, _) => EditSelectedCell();
        edit.Controls.Add(lblSel);
        edit.Controls.Add(new Label { Text = "Root", AutoSize = true, Margin = new Padding(0, 8, 4, 0) });
        edit.Controls.Add(cboRoot);
        edit.Controls.Add(new Label { Text = "Type", AutoSize = true, Margin = new Padding(12, 8, 4, 0) });
        edit.Controls.Add(cboType);
        root.Controls.Add(edit, 0, 2);

        Controls.Add(root);
    }

    private static Button MakeButton(string text, Action onClick)
    {
        var b = new Button { Text = text, AutoSize = true, Margin = new Padding(4, 4, 0, 0) };
        b.Click += (_, _) => onClick();
        return b;
    }

    private Button MakeCellButton(Direction d, bool outer)
    {
        var b = new Button { Dock = DockStyle.Fill, Margin = new Padding(3), FlatStyle = FlatStyle.System, Font = new Font("Segoe UI", 9f) };
        b.Click += (_, _) => SelectCell(d, outer);
        return b;
    }

    private void ReloadPresetList()
    {
        suppress = true;
        cboPreset.Items.Clear();
        foreach (var p in config.Presets) cboPreset.Items.Add(p.Name);
        cboPreset.SelectedIndex = config.ActivePreset;
        suppress = false;
    }

    private void RefreshGrid()
    {
        foreach (var (d, b) in innerBtn) b.Text = (d == Direction.Neutral ? "● " : "") + Active.Inner[d];
        foreach (var (d, b) in outerBtn) b.Text = "dash\n" + Active.Outer[d];
        Repaint();
    }

    private void Repaint()
    {
        foreach (var (d, b) in innerBtn)
            b.BackColor = !selOuter && d == selDir ? Color.LightSkyBlue : d == Direction.Neutral ? Color.LightGoldenrodYellow : SystemColors.Control;
        foreach (var (d, b) in outerBtn)
            b.BackColor = selOuter && d == selDir ? Color.LightSkyBlue : Color.MistyRose;
    }

    private void SelectCell(Direction d, bool outer)
    {
        selDir = d; selOuter = outer;
        Cell cell = outer ? Active.Outer[d] : Active.Inner[d];
        suppress = true;
        cboRoot.SelectedIndex = cell.Root % 12;
        cboType.SelectedItem = cell.Type;
        suppress = false;
        lblSel.Text = (outer ? $"Outer {d} (dash):" : d == Direction.Neutral ? "Center:" : $"{d}:");
        Repaint();
    }

    private void EditSelectedCell()
    {
        if (suppress || cboRoot.SelectedIndex < 0 || cboType.SelectedItem == null) return;
        Cell cell = selOuter ? Active.Outer[selDir] : Active.Inner[selDir];
        cell.Root = cboRoot.SelectedIndex;
        cell.Type = (ChordType)cboType.SelectedItem;
        RefreshGrid();
        main.PushPreset(Active);
        main.Save();
    }

    private void NewPreset()
    {
        string? name = Prompt.Text(this, "New preset", "Name:", $"Preset {config.Presets.Count + 1}");
        if (name == null) return;
        var p = Preset.Default(); p.Name = name;
        config.Presets.Add(p);
        config.ActivePreset = config.Presets.Count - 1;
        ReloadPresetList(); RefreshGrid(); SelectCell(Direction.Neutral, false);
        main.PushPreset(Active); main.Save();
    }

    private void DuplicatePreset()
    {
        var p = Active.Clone();
        p.Name = Active.Name + " copy";
        config.Presets.Add(p);
        config.ActivePreset = config.Presets.Count - 1;
        ReloadPresetList(); RefreshGrid(); SelectCell(Direction.Neutral, false);
        main.PushPreset(Active); main.Save();
    }

    private void RenamePreset()
    {
        string? name = Prompt.Text(this, "Rename preset", "Name:", Active.Name);
        if (name == null) return;
        Active.Name = name;
        ReloadPresetList(); main.Save();
    }

    private void DeletePreset()
    {
        if (config.Presets.Count <= 1) { MessageBox.Show(this, "At least one preset is required."); return; }
        config.Presets.RemoveAt(config.ActivePreset);
        config.ActivePreset = Math.Clamp(config.ActivePreset, 0, config.Presets.Count - 1);
        ReloadPresetList(); RefreshGrid(); SelectCell(Direction.Neutral, false);
        main.PushPreset(Active); main.Save();
    }
}
