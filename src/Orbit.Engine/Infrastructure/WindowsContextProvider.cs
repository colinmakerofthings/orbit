using Orbit.Core.Contracts;
using Orbit.Core.Models;
using System.Runtime.InteropServices;
using System.Text;

namespace Orbit.Engine.Infrastructure;

/// <summary>
/// Detects the currently active window and foreground Explorer folder using Windows APIs.
/// </summary>
public class WindowsContextProvider : IContextProvider
{
    public Task<ContextData> GetCurrentContextAsync()
    {
        var hwnd = GetForegroundWindow();

        var titleBuilder = new StringBuilder(512);
        GetWindowText(hwnd, titleBuilder, titleBuilder.Capacity);
        var windowTitle = titleBuilder.ToString();

        GetWindowThreadProcessId(hwnd, out var pid);
        var process = System.Diagnostics.Process.GetProcessById((int)pid);
        var exeName = process.ProcessName + ".exe";

        var currentFolder = TryGetExplorerFolder(hwnd, exeName)
            ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return Task.FromResult(new ContextData
        {
            ActiveApplication = exeName,
            WindowTitle = windowTitle,
            CurrentFolder = currentFolder
        });
    }

    private static string? TryGetExplorerFolder(nint hwnd, string exeName)
    {
        if (!exeName.Equals("explorer.exe", StringComparison.OrdinalIgnoreCase))
            return null;

        // Query the Shell Windows COM object for the active Explorer folder
        try
        {
            var shellType = Type.GetTypeFromProgID("Shell.Application");
            if (shellType is null) return null;

            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic windows = shell.Windows();

            for (int i = 0; i < windows.Count; i++)
            {
                dynamic w = windows.Item(i);
                try
                {
                    if ((nint)(int)w.HWND == hwnd)
                        return new Uri(w.LocationURL).LocalPath;
                }
                catch { /* window may have closed */ }
            }
        }
        catch { /* COM not available */ }

        return null;
    }

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(nint hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);
}
