using System.Drawing;
using System.Windows.Forms;
using VirtuoPhone.Config;

namespace VirtuoPhone.UI;

/// <summary>Audio settings: output device, shared vs. exclusive, latency, and master volume. "Apply" rebuilds
/// the engine's audio path live (and persists) — replacing the old VP_AUDIO_MODE / VP_LATENCY_MS env vars.</summary>
public sealed class AudioTab : UserControl
{
    private sealed record DeviceItem(string Name, string? Id) { public override string ToString() => Name; }

    private readonly MainForm main;
    private readonly AppConfig config;

    private ComboBox cboDevice = null!;
    private RadioButton rbShared = null!, rbExclusive = null!;
    private NumericUpDown numLatency = null!;
    private TrackBar tbVolume = null!;
    private Label lblVol = null!;

    public AudioTab(MainForm main)
    {
        this.main = main;
        config = main.Config;
        Dock = DockStyle.Fill;
        Build();
        LoadFromConfig();
    }

    private void Build()
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(14), AutoSize = true };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        cboDevice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 400 };
        AddRow(t, "Output device", cboDevice);

        var modePanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        rbShared = new RadioButton { Text = "Shared (Windows volume works)", AutoSize = true, Checked = true };
        rbExclusive = new RadioButton { Text = "Exclusive (44.1 kHz bit-exact)", AutoSize = true, Margin = new Padding(16, 0, 0, 0) };
        modePanel.Controls.Add(rbShared);
        modePanel.Controls.Add(rbExclusive);
        AddRow(t, "Mode", modePanel);

        numLatency = new NumericUpDown { Minimum = 3, Maximum = 200, Value = 10, Width = 70 };
        AddRow(t, "Latency (ms)", numLatency);

        var volPanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        tbVolume = new TrackBar { Minimum = 0, Maximum = 100, Value = 100, Width = 260, TickFrequency = 10 };
        lblVol = new Label { Text = "100%", AutoSize = true, Margin = new Padding(8, 8, 0, 0) };
        tbVolume.Scroll += (_, _) => lblVol.Text = tbVolume.Value + "%";
        volPanel.Controls.Add(tbVolume);
        volPanel.Controls.Add(lblVol);
        AddRow(t, "Master volume", volPanel);

        var apply = new Button { Text = "Apply", AutoSize = true, Margin = new Padding(0, 8, 0, 0) };
        apply.Click += (_, _) => Apply();
        AddRow(t, "", apply);

        AddRow(t, "", new Label
        {
            AutoSize = true, MaximumSize = new Size(560, 0), ForeColor = Color.DimGray,
            Text = "Exclusive mode gives the lowest latency and a bit-exact 44.1 kHz stream, but bypasses the "
                 + "Windows volume mixer and takes the device exclusively. If a setting can't be opened, the "
                 + "engine falls back to a safe one.",
        });

        Controls.Add(t);
    }

    private static void AddRow(TableLayoutPanel t, string caption, Control control)
    {
        int row = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0) }, 0, row);
        control.Margin = new Padding(0, 5, 0, 5);
        t.Controls.Add(control, 1, row);
    }

    private void LoadFromConfig()
    {
        cboDevice.Items.Add(new DeviceItem("(System default)", null));
        foreach (var d in AudioDevices.List()) cboDevice.Items.Add(new DeviceItem(d.Name, d.Id));
        int sel = 0;
        for (int i = 0; i < cboDevice.Items.Count; i++)
            if (((DeviceItem)cboDevice.Items[i]!).Id == config.Audio.DeviceId) { sel = i; break; }
        cboDevice.SelectedIndex = sel;

        rbExclusive.Checked = config.Audio.Exclusive;
        rbShared.Checked = !config.Audio.Exclusive;
        numLatency.Value = Math.Clamp(config.Audio.LatencyMs, (int)numLatency.Minimum, (int)numLatency.Maximum);
        tbVolume.Value = Math.Clamp((int)Math.Round(config.Audio.MasterVolume * 100), 0, 100);
        lblVol.Text = tbVolume.Value + "%";
    }

    private void Apply()
    {
        config.Audio.DeviceId = ((DeviceItem)cboDevice.SelectedItem!).Id;
        config.Audio.Exclusive = rbExclusive.Checked;
        config.Audio.LatencyMs = (int)numLatency.Value;
        config.Audio.MasterVolume = tbVolume.Value / 100f;
        main.PushAudio(config.Audio);
        main.Save();
    }
}
