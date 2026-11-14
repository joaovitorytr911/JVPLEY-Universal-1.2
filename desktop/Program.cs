using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace JVRSMPLAY.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        string pagePath = Path.Combine(AppContext.BaseDirectory, "app", "index.html");
        if (!File.Exists(pagePath))
        {
            MessageBox.Show(
                "Não encontrei app\\index.html. Reinstale o JVRSMPLAY usando o pacote completo.",
                "JVRSMPLAY",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var window = new Form
        {
            Text = "JVRSMPLAY",
            WindowState = FormWindowState.Maximized,
            StartPosition = FormStartPosition.CenterScreen,
            MinimumSize = new Size(900, 600),
            BackColor = Color.FromArgb(3, 9, 16)
        };

        var browser = new WebView2 { Dock = DockStyle.Fill };
        window.Controls.Add(browser);

        window.Shown += async (_, _) =>
        {
            try
            {
                await browser.EnsureCoreWebView2Async();

                // Keep the multimedia interface inside the app. Open web services
                // in the user's default browser instead of creating a blank WebView.
                browser.CoreWebView2.NewWindowRequested += (_, e) =>
                {
                    e.Handled = true;
                    OpenExternalUrl(e.Uri);
                };

                browser.CoreWebView2.NavigationStarting += (_, e) =>
                {
                    if (!Uri.TryCreate(e.Uri, UriKind.Absolute, out var uri))
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (uri.IsFile && string.Equals(
                        Path.GetFullPath(uri.LocalPath),
                        Path.GetFullPath(pagePath),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    if (uri.Scheme is "https" or "http")
                    {
                        e.Cancel = true;
                        OpenExternalUrl(uri.ToString());
                        return;
                    }

                    e.Cancel = true;
                };

                browser.Source = new Uri(pagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível iniciar o componente WebView2. Verifique se o Microsoft Edge WebView2 Runtime está instalado.\\n\\n" + ex.Message,
                    "JVRSMPLAY - inicialização",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                window.Close();
            }
        };

        Application.Run(window);
    }

    private static void OpenExternalUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("https" or "http"))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(uri.ToString()) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível abrir o link no navegador padrão.\\n\\n" + ex.Message,
                "JVRSMPLAY",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
