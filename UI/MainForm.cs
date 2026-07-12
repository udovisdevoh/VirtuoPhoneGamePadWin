using System.Drawing;
using System.Windows.Forms;
using VirtuoPhone.Config;
using VirtuoPhone.Engine;
using VirtuoPhone.Input;

namespace VirtuoPhone.UI;

/// <summary>
/// The application window. Owns the config, the mapped controller and the (background) <see cref="PlayEngine"/>,
/// and hosts four tabs: live Play, the Preset editor, the Controls remap, and Audio settings. The engine runs
/// off the UI thread; its events are marshaled back here to update the display.
/// </summary>
public sealed class MainForm : Form
{
    private readonly AppConfig config;
    private readonly MappedControllerInput input;
    private readonly int deviceId;
    private PlayEngine? engine;

    // Play tab
    private Button btnStartStop = null!;
    private Label lblInput = null!, lblInstrument = null!, lblCenter = null!, lblChord = null!, lblDir = null!, lblLayout = null!;
    private TextBox txtLog = null!;

    public AppConfig Config => config;
    public MappedControllerInput Input => input;
    public int DeviceId => deviceId;
    public bool EngineRunning => engine?.IsRunning == true;

    public MainForm()
    {
        config = ConfigStore.Load();
        AudioBackend.Settings = config.Audio;
        deviceId = WinmmControllerInput.FindFirstConnected();
        input = new MappedControllerInput(deviceId, config.Controls);

        Text = "VirtuoPhone — arcade-stick synth";
        MinimumSize = new Size(760, 560);
        Size = new Size(860, 620);
        Font = new Font("Segoe UI", 9f);

        BuildTabs();
        FormClosing += (_, _) => { StopEngine(); Save(); };
    }

    private void BuildTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill };

        var play = new TabPage("Play");
        play.Controls.Add(BuildPlayTab());
        tabs.TabPages.Add(play);

        var presets = new TabPage("Presets");
        presets.Controls.Add(new PresetTab(this) { Dock = DockStyle.Fill });
        tabs.TabPages.Add(presets);

        var controls = new TabPage("Controls");
        controls.Controls.Add(new ControlsTab(this) { Dock = DockStyle.Fill });
        tabs.TabPages.Add(controls);

        var audio = new TabPage("Audio");
        audio.Controls.Add(new AudioTab(this) { Dock = DockStyle.Fill });
        tabs.TabPages.Add(audio);

        Controls.Add(tabs);
    }

    private Control BuildPlayTab()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(12) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        btnStartStop = new Button { Text = "▶ Start", Width = 120, Height = 32, Font = new Font("Segoe UI", 10f, FontStyle.Bold) };
        btnStartStop.Click += (_, _) => { if (EngineRunning) StopEngine(); else StartEngine(); };
        lblInput = new Label { Text = $"Input: {input.Name}", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(16, 8, 0, 0) };
        var top = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
        top.Controls.Add(btnStartStop);
        top.Controls.Add(lblInput);
        root.Controls.Add(top, 0, 0);

        var status = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 8) };
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        status.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        lblInstrument = AddStatusRow(status, "Instrument");
        lblCenter = AddStatusRow(status, "Center");
        lblChord = AddStatusRow(status, "Chord");
        lblDir = AddStatusRow(status, "Direction");
        lblLayout = AddStatusRow(status, "Layout");
        root.Controls.Add(status, 0, 1);

        txtLog = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill,
                               BackColor = Color.FromArgb(24, 24, 24), ForeColor = Color.Gainsboro, Font = new Font("Consolas", 9f) };
        root.Controls.Add(txtLog, 0, 2);
        return root;
    }

    private static Label AddStatusRow(TableLayoutPanel t, string caption)
    {
        int row = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(new Label { Text = caption + ":", AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Anchor = AnchorStyles.Left }, 0, row);
        var val = new Label { Text = "—", AutoSize = true, Anchor = AnchorStyles.Left };
        t.Controls.Add(val, 1, row);
        return val;
    }

    private void StartEngine()
    {
        input.SetMap(config.Controls);
        AudioBackend.Settings = config.Audio;
        engine = new PlayEngine(input, config.ActivePresetOrDefault(), config.Instrument);
        engine.Status += m => Ui(() => AppendLog(m));
        engine.StateChanged += s => Ui(() => ShowState(s));
        engine.Start();
        btnStartStop.Text = "■ Stop";
        AppendLog("[engine] started");
    }

    private void StopEngine()
    {
        if (engine == null) return;
        engine.Stop();
        config.Instrument = AppController.GetAppController().InstrumentIndex;   // remember last instrument
        engine = null;
        btnStartStop.Text = "▶ Start";
        AppendLog("[engine] stopped");
    }

    private void ShowState(EngineState s)
    {
        lblInstrument.Text = s.Instrument;
        lblCenter.Text = s.Center;
        lblChord.Text = s.Chord + (s.Dash ? "   (dash)" : "");
        lblDir.Text = s.Direction.ToString();
        lblLayout.Text = s.Layout;
    }

    private void AppendLog(string m)
    {
        if (txtLog.TextLength > 12000) txtLog.Clear();
        txtLog.AppendText(m + Environment.NewLine);
    }

    /// <summary>Marshal an action onto the UI thread (engine events fire on the engine thread).</summary>
    private void Ui(Action a)
    {
        if (!IsHandleCreated || IsDisposed) return;
        try { BeginInvoke(a); } catch { /* form closing */ }
    }

    // ---- Called by the tab controls when the user changes settings ----

    /// <summary>Push an edited preset to the running engine (no-op if stopped; it loads on next Start).</summary>
    public void PushPreset(Preset p) => engine?.SetPreset(p);

    /// <summary>Apply audio settings now: for a running engine, live-rebuild; also affects the next Start.</summary>
    public void PushAudio(AudioSettings a)
    {
        AudioBackend.Settings = a;
        engine?.SetAudioSettings(a);
    }

    /// <summary>Apply the (edited) controller map to the live input reader.</summary>
    public void PushControls() => input.SetMap(config.Controls);

    public void Save() => ConfigStore.Save(config);
}
