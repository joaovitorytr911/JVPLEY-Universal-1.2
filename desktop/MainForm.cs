using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace CARPLEY.Desktop;

public sealed class MainForm : Form
{
    private readonly WebView2 browser = new();
    private readonly string pagePath;
    private bool fullScreen = true;
    private FormBorderStyle previousBorder;
    private FormWindowState previousState;
    private Rectangle previousBounds;

    public MainForm()
    {
        Text = "CARPLEY Universal 1.3";
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(3, 9, 16);
        MinimumSize = new Size(900, 540);
        pagePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "app", "index.html"));

        browser.Dock = DockStyle.Fill;
        browser.DefaultBackgroundColor = Color.FromArgb(3, 9, 16);
        Controls.Add(browser);

        Shown += async (_, _) => await StartCentralAsync();
        KeyPreview = true;
        KeyDown += HandleKeys;
    }

    private async Task StartCentralAsync()
    {
        try
        {
            if (!File.Exists(pagePath))
            {
                MessageBox.Show(
                    "Não encontrei app/index.html. Confira a pasta do CARPLEY.",
                    "CARPLEY", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await browser.EnsureCoreWebView2Async();
            browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            browser.CoreWebView2.Settings.AreDevToolsEnabled = false;
            browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
            browser.Source = new Uri(pagePath);
            EnterFullScreen();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível iniciar a central. Verifique se o Microsoft Edge WebView2 Runtime está instalado.\n\n" + ex.Message,
                "CARPLEY - falha ao iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void HandleKeys(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F11)
        {
            if (fullScreen) ExitFullScreen();
            else EnterFullScreen();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Escape && fullScreen)
        {
            ExitFullScreen();
            e.Handled = true;
        }
    }

    private void EnterFullScreen()
    {
        previousBorder = FormBorderStyle;
        previousState = WindowState;
        previousBounds = Bounds;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        fullScreen = true;
    }

    private void ExitFullScreen()
    {
        FormBorderStyle = previousBorder;
        WindowState = FormWindowState.Normal;
        Bounds = previousBounds;
        WindowState = previousState;
        fullScreen = false;
    }
}
