using Microsoft.Web.WebView2.WinForms;
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
}