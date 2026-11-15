using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Speech.Synthesis;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace JVRSMPLAY.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var splash = new SplashForm();
        if (splash.ShowDialog() != DialogResult.OK) return;
        Application.Run(new JvrsmMainForm());
    }
}

internal sealed class SplashForm : Form
{
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 5000 };
    public SplashForm()
    {
        Text = "JVRSMPLAY";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        BackColor = Color.Black;
        var imagePath = Path.Combine(AppContext.BaseDirectory, "JVRSMPLAY-Splash.jpg");
        if (File.Exists(imagePath))
        {
            var picture = new PictureBox { Dock = DockStyle.Fill, Image = Image.FromFile(imagePath), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };
            Controls.Add(picture);
            picture.Click += (_, _) => Continue();
        }
        var enter = new Button
        {
            Text = "ENTRAR",
            Dock = DockStyle.Bottom,
            Height = 64,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(5, 18, 35),
            ForeColor = Color.FromArgb(65, 220, 255),
            Font = new Font("Segoe UI", 18, FontStyle.Bold)
        };
        enter.FlatAppearance.BorderColor = Color.DeepSkyBlue;
        enter.Click += (_, _) => Continue();
        Controls.Add(enter);
        enter.BringToFront();
        timer.Tick += (_, _) => Continue();
        Shown += (_, _) => timer.Start();
        FormClosed += (_, _) => timer.Stop();
        KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape) Continue(); };
        KeyPreview = true;
    }
    private void Continue()
    {
        timer.Stop();
        DialogResult = DialogResult.OK;
        Close();
    }
}

internal sealed class JvrsmMainForm : Form
{
    private static readonly Color BackgroundColor = Color.FromArgb(2, 7, 16);
    private static readonly Color PanelColor = Color.FromArgb(5, 25, 43);
    private static readonly Color AccentColor = Color.FromArgb(35, 194, 255);
    private readonly Label clockLabel = new();
    private readonly Label consumptionLabel = new();
    private readonly Label telemetryStatus = new();
    private readonly TrackBar volume = new();
    private readonly SpeechSynthesizer speech = new();
    private readonly TelemetryPipeClient telemetry = new();
    private readonly CancellationTokenSource telemetryCancellation = new();

    public JvrsmMainForm()
    {
        Text = "JVRSMPLAY";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(900, 620);
        BackColor = BackgroundColor;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 10F);
        BuildInterface();
        ConfigureSpeech();
        var clock = new System.Windows.Forms.Timer { Interval = 1000 };
        clock.Tick += (_, _) => clockLabel.Text = DateTime.Now.ToString("HH:mm");
        clock.Start();
        clockLabel.Text = DateTime.Now.ToString("HH:mm");
        Shown += (_, _) => _ = telemetry.RunAsync(
            v => UpdateOnUi(() => consumptionLabel.Text = $"{v.ToString("N1", CultureInfo.GetCultureInfo("pt-BR"))} L/100 km"),
            () => UpdateOnUi(() => consumptionLabel.Text = "-- L/100 km"),
            connected => UpdateOnUi(() => telemetryStatus.Text = connected ? "Plugin ETS2 conectado." : "Aguardando conexão com o ETS2."),
            telemetryCancellation.Token);
        FormClosed += (_, _) => { telemetryCancellation.Cancel(); speech.Dispose(); clock.Stop(); clock.Dispose(); telemetryCancellation.Dispose(); };
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F2) OpenWebApp("https://www.youtube.com/", "YouTube");
            if (e.KeyCode == Keys.F3) OpenWebApp("https://open.spotify.com/", "Spotify");
            if (e.KeyCode == Keys.F4) Radio();
            if (e.KeyCode == Keys.F5) OpenWebApp("https://www.google.com/maps", "GPS");
        };
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(26), BackColor = BackgroundColor, ColumnCount = 1, RowCount = 5 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 22));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
        Controls.Add(root);

        var header = new Panel { Dock = DockStyle.Fill };
        var brand = new Label { Text = "JVRSMPLAY", ForeColor = AccentColor, Font = new Font("Segoe UI", 26, FontStyle.Bold), AutoSize = true, Location = new Point(0, 2) };
        var subtitle = new Label { Text = "MULTIMÍDIA UNIVERSAL", ForeColor = Color.LightSkyBlue, Font = new Font("Segoe UI", 8, FontStyle.Bold), AutoSize = true, Location = new Point(3, 48) };
        clockLabel.Font = new Font("Segoe UI", 21, FontStyle.Bold); clockLabel.ForeColor = Color.White; clockLabel.AutoSize = true; clockLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        header.Resize += (_, _) => clockLabel.Location = new Point(Math.Max(0, header.ClientSize.Width - 110), 12);
        header.Controls.AddRange(new Control[] { brand, subtitle, clockLabel }); root.Controls.Add(header, 0, 0);

        var status = Panel();
        var welcome = Label("SISTEMA MULTIMÍDIA", 24, AccentColor, true); welcome.Location = new Point(26, 24);
        var info = Label("Bluetooth • entretenimento • navegação", 13, Color.LightSkyBlue, false); info.Location = new Point(29, 68);
        var note = Label("Selecione uma função nos seis atalhos abaixo.", 11, Color.White, false); note.Location = new Point(29, 104);
        status.Controls.AddRange(new Control[] { welcome, info, note }); root.Controls.Add(status, 0, 1);

        var audio = Panel();
        var audioTitle = Label("VOLUME", 16, Color.White, true); audioTitle.Location = new Point(22, 14);
        volume.Minimum = 0; volume.Maximum = 100; volume.Value = 70; volume.TickFrequency = 10; volume.Width = 320; volume.Location = new Point(20, 44); volume.BackColor = PanelColor;
        var volumeValue = Label("70%", 14, AccentColor, true); volumeValue.Location = new Point(350, 50);
        volume.ValueChanged += (_, _) => { speech.Volume = volume.Value; volumeValue.Text = $"{volume.Value}%"; };
        audio.Controls.AddRange(new Control[] { audioTitle, volume, volumeValue }); root.Controls.Add(audio, 0, 2);

        var telemetryPanel = Panel();
        var telemetryTitle = Label("CONSUMO ETS2", 16, Color.White, true); telemetryTitle.Location = new Point(22, 10);
        consumptionLabel.Text = "-- L/100 km"; consumptionLabel.Font = new Font("Segoe UI", 23, FontStyle.Bold); consumptionLabel.ForeColor = AccentColor; consumptionLabel.AutoSize = true; consumptionLabel.Location = new Point(22, 42);
        telemetryStatus.Text = "Aguardando conexão com o ETS2."; telemetryStatus.Font = new Font("Segoe UI", 9); telemetryStatus.ForeColor = Color.LightSkyBlue; telemetryStatus.AutoSize = true; telemetryStatus.Location = new Point(26, 90);
        telemetryPanel.Controls.AddRange(new Control[] { telemetryTitle, consumptionLabel, telemetryStatus }); root.Controls.Add(telemetryPanel, 0, 3);

        var apps = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = BackgroundColor, ColumnCount = 6, RowCount = 1, Padding = new Padding(0, 8, 0, 0) };
        for (int i = 0; i < 6; i++) apps.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 6));
        AddApp(apps, "ᛒ\r\nBluetooth", () => Open("ms-settings:bluetooth"));
        AddApp(apps, "♫\r\nSpotify", () => OpenWebApp("https://open.spotify.com/", "Spotify"));
        AddApp(apps, "▶\r\nYouTube", () => OpenWebApp("https://www.youtube.com/", "YouTube"));
        AddApp(apps, "◖))\r\nRádio FM", Radio);
        AddApp(apps, "⌖\r\nGPS", () => { Speak("Abrindo navegação."); OpenWebApp("https://www.google.com/maps", "GPS"); });
        AddApp(apps, "⚙\r\nConfigurações", () => MessageBox.Show("Configurações do JVRSMPLAY.", "JVRSMPLAY", MessageBoxButtons.OK, MessageBoxIcon.Information));
        root.Controls.Add(apps, 0, 4);
    }

    private Panel Panel() => new() { Dock = DockStyle.Fill, BackColor = PanelColor, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 7, 0, 7) };
    private Label Label(string text, float size, Color color, bool bold) => new() { Text = text, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), ForeColor = color, AutoSize = true };
    private void AddApp(TableLayoutPanel host, string text, Action action)
    {
        var button = new Button { Text = text, Dock = DockStyle.Fill, Margin = new Padding(4), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(5, 61, 91), ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand };
        button.FlatAppearance.BorderColor = AccentColor; button.Click += (_, _) => action(); host.Controls.Add(button);
    }
    private void ConfigureSpeech()
    {
        try { var voice = speech.GetInstalledVoices().FirstOrDefault(v => v.Enabled && v.VoiceInfo.Culture.Name.StartsWith("pt-BR", StringComparison.OrdinalIgnoreCase)); if (voice is not null) speech.SelectVoice(voice.VoiceInfo.Name); }
        catch { }
        speech.Volume = 70;
    }
    private void Speak(string text) { try { speech.SpeakAsyncCancelAll(); speech.SpeakAsync(text); } catch { } }
    private void Radio() => MessageBox.Show("Rádio FM: as estações ainda precisam ser configuradas.", "JVRSMPLAY", MessageBoxButtons.OK, MessageBoxIcon.Information);
    private static void Open(string target) { try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); } catch (Exception ex) { MessageBox.Show(ex.Message, "JVRSMPLAY"); } }
    private void UpdateOnUi(Action update) { if (IsDisposed || !IsHandleCreated) return; try { BeginInvoke(update); } catch (InvalidOperationException) { } }
    private void OpenWebApp(string url, string title)
    {
        var browser = new WebAppForm(url, title);
        browser.Show(this);
    }
}

internal sealed class WebAppForm : Form
{
    private readonly string startUrl;
    private readonly WebView2 webView = new() { Dock = DockStyle.Fill };
    private readonly Label status = new()
    {
        Text = "Preparando o navegador integrado...",
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        ForeColor = Color.LightSkyBlue,
        BackColor = Color.FromArgb(2, 7, 16)
    };

    public WebAppForm(string url, string title)
    {
        startUrl = url;
        Text = $"JVRSMPLAY — {title}";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(800, 550);
        BackColor = Color.FromArgb(2, 7, 16);

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            BackColor = Color.FromArgb(5, 25, 43),
            Padding = new Padding(8),
            WrapContents = false
        };
        AddToolbarButton(toolbar, "←", () => { if (webView.CoreWebView2?.CanGoBack == true) webView.CoreWebView2.GoBack(); });
        AddToolbarButton(toolbar, "→", () => { if (webView.CoreWebView2?.CanGoForward == true) webView.CoreWebView2.GoForward(); });
        AddToolbarButton(toolbar, "↻", () => webView.CoreWebView2?.Reload());
        AddToolbarButton(toolbar, "Início", () => webView.CoreWebView2?.Navigate(startUrl));

        Controls.Add(webView);
        Controls.Add(status);
        Controls.Add(toolbar);
        status.BringToFront();
        Shown += async (_, _) => await InitializeBrowserAsync();
    }

    private static void AddToolbarButton(FlowLayoutPanel toolbar, string caption, Action action)
    {
        var button = new Button
        {
            Text = caption,
            AutoSize = true,
            Height = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(5, 61, 91),
            ForeColor = Color.White,
            Margin = new Padding(4, 0, 4, 0)
        };
        button.FlatAppearance.BorderColor = Color.DeepSkyBlue;
        button.Click += (_, _) => action();
        toolbar.Controls.Add(button);
    }

    private async Task InitializeBrowserAsync()
    {
        try
        {
            var dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "JVRSMPLAY", "WebView2");
            Directory.CreateDirectory(dataFolder);
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: dataFolder);
            await webView.EnsureCoreWebView2Async(environment);
            webView.CoreWebView2.Navigate(startUrl);
            status.Visible = false;
        }
        catch (Exception ex)
        {
            status.Text = "Não foi possível iniciar o navegador integrado.\r\n" +
                "Verifique se o Microsoft Edge WebView2 Runtime está instalado.\r\n\r\n" +
                ex.Message;
            var install = MessageBox.Show(
                "O WebView2 Runtime pode estar ausente. Deseja abrir a página oficial da Microsoft para instalá-lo?",
                "JVRSMPLAY — WebView2",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (install == DialogResult.Yes)
            {
                try { Process.Start(new ProcessStartInfo("https://developer.microsoft.com/microsoft-edge/webview2/") { UseShellExecute = true }); }
                catch { }
            }
        }
    }
}

