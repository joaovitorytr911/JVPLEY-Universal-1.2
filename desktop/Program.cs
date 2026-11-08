using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CARPLEY.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        string pagePath = Path.Combine(AppContext.BaseDirectory, "app", "index.html");
        if (!File.Exists(pagePath))
        {
            ShowMessage("Não encontrei a pasta app\\index.html. Extraia todos os arquivos do pacote antes de iniciar.", "CARPLEY");
            return;
        }

        string pageUrl = new Uri(pagePath).AbsoluteUri;
        string[] browsers =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe")
        };

        try
        {
            string? browser = browsers.FirstOrDefault(File.Exists);
            if (browser is not null)
            {
                var start = new ProcessStartInfo(browser) { UseShellExecute = false };
                start.ArgumentList.Add("--app=" + pageUrl);
                start.ArgumentList.Add("--start-fullscreen");
                start.ArgumentList.Add("--no-first-run");
                Process.Start(start);
                return;
            }

            Process.Start(new ProcessStartInfo(pageUrl) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowMessage("Não foi possível abrir a central CARPLEY.\n\n" + ex.Message, "CARPLEY - falha ao iniciar");
        }
    }

    private static void ShowMessage(string message, string title) =>
        MessageBox(IntPtr.Zero, message, title, 0x10);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
}