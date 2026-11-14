using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace JVRSMPLAY.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new JvrsmMainForm());
    }
}

internal sealed class JvrsmMainForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(3, 9, 16);
    private static readonly Color PanelColor = Color.FromArgb(7, 27, 43);
    private static readonly Color BorderColor = Color.FromArgb(23, 101, 139);
    private static readonly Color AccentColor = Color.FromArgb(53, 202, 255);
    private readonly Label clockLabel = new();
    private readonly Label consumptionLabel = new();
    private readonly Label telemetryStatus = new();
    private readonly Label assistantText = new();
    private readonly TrackBar voiceVolume = new();
    private readonly SpeechSynthesizer speech = new();
    private readonly TelemetryPipeClient telemetry = new();
    private readonly CancellationTokenSource telemetryCancellation = new();
    private readonly List<Button> appButtons = new();
    private int selectedApp;

    public JvrsmMainForm()
    {
        Text = "JVRSMPLAY";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(900, 650);
        BackColor = BackgroundColor;
        ForeColor = Color.FromArgb(232, 248, 255);
        Font = new Font("Segoe UI", 10F);
        KeyPreview = true;
        BuildInterface();
        ConfigureSpeech();

        var clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        clockTimer.Tick += (_, _) => clockLabel.Text = DateTime.Now.ToString("HH:mm");
        clockTimer.Start();
        clockLabel.Text = DateTime.Now.ToString("HH:mm");

        Shown += (_, _) => _ = telemetry.RunAsync(
            value => UpdateOnUi(() => consumptionLabel.Text = $"{value.ToString("N1", CultureInfo.GetCultureInfo("pt-BR"))} L/100 km"),
            () => UpdateOnUi(() => consumptionLabel.Text = "-- L/100 km"),
            connected => UpdateOnUi(() => telemetryStatus.Text = connected
                ? "Plugin ETS2 conectado. Exibindo consumo recebido do jogo."
                : "Telemetria desconectada. Aguardando conexão com o ETS2."),
            telemetryCancellation.Token);

        FormClosed += (_, _) =>
        {
            telemetryCancellation.Cancel();
            speech.Dispose();
            clockTimer.Stop();
            clockTimer.Dispose();
            telemetryCancellation.Dispose();
        };

        KeyDown += HandleShortcut;
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            Padding = new Padding(28),
            ColumnCount = 1,
            RowCount = 6
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 23));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 28));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 14));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill, BackColor = BackgroundColor };
        var brand = new Label
        {
            Text = "JVRSMPLAY",
            ForeColor = AccentColor,
            Font = new Font("Segoe UI", 25, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(0, 3)
        };
        var subtitle = new Label
        {
            Text = "MULTIMÍDIA UNIVERSAL",
            ForeColor = Color.FromArgb(120, 185, 209),
            Font = new Font("Segoe UI", 8, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(3, 48)
        };
        clockLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
        clockLabel.ForeColor = Color.White;
        clockLabel.AutoSize = true;
        clockLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        clockLabel.Location = new Point(Math.Max(0, ClientSize.Width - 130), 15);
        header.Resize += (_, _) => clockLabel.Location = new Point(header.ClientSize.Width - 100, 15);
        header.Controls.Add(brand);
        header.Controls.Add(subtitle);
        header.Controls.Add(clockLabel);
        root.Controls.Add(header, 0, 0);

        var hero = MakePanel();
        hero.Padding = new Padding(28);
        var orb = new Label
        {
            Text = "S",
            ForeColor = Color.FromArgb(185, 241, 255),
            BackColor = Color.FromArgb(8, 55, 80),
            Font = new Font("Segoe UI", 54, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(140, 140),
            Location = new Point(28, 28),
            BorderStyle = BorderStyle.FixedSingle
        };
        var eyebrow = MakeLabel("ASSISTENTE DE BORDO", 10, AccentColor, true);
        eyebrow.Location = new Point(205, 35);
        var truck = MakeLabel("SCANIA", 34, Color.White, true);
        truck.Location = new Point(202, 58);
        assistantText.Text = "Olá! Eu sou a SCANIA. Como posso ajudar na sua viagem?";
        assistantText.ForeColor = Color.FromArgb(182, 213, 226);
        assistantText.AutoSize = true;
        assistantText.Location = new Point(205, 115);
        var voiceButton = MakeButton("TESTAR VOZ");
        voiceButton.Location = new Point(205, 155);
        voiceButton.Size = new Size(190, 42);
        voiceButton.Click += (_, _) => Speak("Olá! Eu sou a SCANIA. Como posso ajudar na sua viagem?");
        hero.Controls.AddRange(new Control[] { orb, eyebrow, truck, assistantText, voiceButton });
        root.Controls.Add(hero, 0, 1);

        var apps = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = BackgroundColor,
            ColumnCount = 4,
            RowCount = 1,
            Padding = new Padding(0, 12, 0, 8)
        };
        for (var i = 0; i < 4; i++) apps.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        AddAppButton(apps, "▶\r\nYouTube", "https://www.youtube.com/", 0);
        AddAppButton(apps, "♫\r\nSpotify", "https://open.spotify.com/", 1);
        AddAppButton(apps, "◖))\r\nRádio BR FM", null, 2);
        AddAppButton(apps, "⌖\r\nMaps BR", "https://www.google.com/maps", 3);
        root.Controls.Add(apps, 0, 2);

        var audio = MakePanel();
        var audioTitle = MakeLabel("CONTROLE DE VOLUME", 17, Color.White, true);
        audioTitle.Location = new Point(24, 22);
        var audioNote = MakeLabel("Ajusta somente a voz e os sons do próprio JVRSMPLAY.", 9, Color.FromArgb(120, 155, 170), false);
        audioNote.Location = new Point(24, 55);
        voiceVolume.Minimum = 0;
        voiceVolume.Maximum = 100;
        voiceVolume.Value = 70;
        voiceVolume.TickFrequency = 10;
        voiceVolume.Width = 330;
        voiceVolume.Location = new Point(20, 85);
        voiceVolume.BackColor = PanelColor;
        var volumeValue = MakeLabel("70%", 15, AccentColor, true);
        volumeValue.Location = new Point(365, 91);
        voiceVolume.ValueChanged += (_, _) =>
        {
            speech.Volume = voiceVolume.Value;
            volumeValue.Text = $"{voiceVolume.Value}%";
        };
        speech.Volume = 70;
        audio.Controls.AddRange(new Control[] { audioTitle, audioNote, voiceVolume, volumeValue });
        root.Controls.Add(audio, 0, 3);

        var vehicle = MakePanel();
        var vehicleTitle = MakeLabel("TELEMETRIA ETS2", 17, Color.White, true);
        vehicleTitle.Location = new Point(24, 18);
        consumptionLabel.Text = "-- L/100 km";
        consumptionLabel.Font = new Font("Segoe UI", 24, FontStyle.Bold);
        consumptionLabel.ForeColor = AccentColor;
        consumptionLabel.AutoSize = true;
        consumptionLabel.Location = new Point(24, 55);
        telemetryStatus.Text = "Aguardando conexão com o ETS2. Nenhum dado está sendo simulado.";
        telemetryStatus.Font = new Font("Segoe UI", 9);
        telemetryStatus.ForeColor = Color.FromArgb(120, 155, 170);
        telemetryStatus.AutoSize = true;
        telemetryStatus.Location = new Point(28, 110);
        vehicle.Controls.AddRange(new Control[] { vehicleTitle, consumptionLabel, telemetryStatus });
        root.Controls.Add(vehicle, 0, 4);

        var footer = MakeLabel("ATALHOS: F2 YouTube • F3 Spotify • F4 Rádio • F5 Maps     JVRSMPLAY", 8, Color.FromArgb(130, 170, 189), false);
        footer.Dock = DockStyle.Fill;
        footer.TextAlign = ContentAlignment.MiddleLeft;
        root.Controls.Add(footer, 0, 5);
    }

    private Panel MakePanel() => new()
    {
        Dock = DockStyle.Fill,
        BackColor = PanelColor,
        BorderStyle = BorderStyle.FixedSingle,
        Margin = new Padding(0, 8, 0, 8)
    };

    private Label MakeLabel(string text, float size, Color color, bool bold) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
        ForeColor = color,
        AutoSize = true
    };

    private Button MakeButton(string text) => new()
    {
        Text = text,
        FlatStyle = FlatStyle.Flat,
        BackColor = Color.FromArgb(7, 62, 91),
        ForeColor = Color.FromArgb(223, 250, 255),
        FlatAppearance = { BorderColor = AccentColor },
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        Cursor = Cursors.Hand
    };

    private void AddAppButton(TableLayoutPanel host, string text, string? url, int index)
    {
        var button = MakeButton(text);
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(6);
        button.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        button.Click += (_, _) =>
        {
            selectedApp = index;
            if (url is null)
            {
                Speak("Rádio BR FM selecionada. A estação ainda precisa ser configurada.");
                return;
            }
            OpenExternalUrl(url);
        };
        appButtons.Add(button);
        host.Controls.Add(button, index, 0);
    }

    private void ConfigureSpeech()
    {
        try
        {
            var voice = speech.GetInstalledVoices()
                .FirstOrDefault(v => v.Enabled && v.VoiceInfo.Culture.Name.StartsWith("pt-BR", StringComparison.OrdinalIgnoreCase));
            if (voice is not null) speech.SelectVoice(voice.VoiceInfo.Name);
        }
        catch { /* Speech remains optional; the native app still opens. */ }
    }

    private void Speak(string message)
    {
        assistantText.Text = message;
        try
        {
            speech.SpeakAsyncCancelAll();
            speech.SpeakAsync(message);
        }
        catch { /* Missing system speech voices must not prevent app startup. */ }
    }

    private void HandleShortcut(object? sender, KeyEventArgs e)
    {
        var index = e.KeyCode switch
        {
            Keys.F2 => 0,
            Keys.F3 => 1,
            Keys.F4 => 2,
            Keys.F5 => 3,
            _ => -1
        };
        if (index < 0) return;
        e.Handled = true;
        e.SuppressKeyPress = true;
        appButtons[index].PerformClick();
    }

    private void OpenExternalUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch (Exception ex)
        {
            MessageBox.Show($"Não foi possível abrir o link.\n{ex.Message}", "JVRSMPLAY", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void UpdateOnUi(Action update)
    {
        if (IsDisposed || !IsHandleCreated) return;
        try { BeginInvoke(update); } catch (InvalidOperationException) { }
    }
}
