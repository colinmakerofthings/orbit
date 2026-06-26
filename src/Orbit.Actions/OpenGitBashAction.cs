using Orbit.Core.Contracts;
using Orbit.Core.Models;
using System.Diagnostics;

namespace Orbit.Actions;

public class OpenGitBashAction : IAction
{
    public string Name => "open-gitbash";

    public Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        var folder = ResolveFolder(parameters, context);

        var gitBashPath = FindGitBash();
        if (gitBashPath is null)
            return Task.FromResult(ActionResult.Fail("Git Bash not found. Ensure Git for Windows is installed."));

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = gitBashPath,
                WorkingDirectory = Directory.Exists(folder) ? folder : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                UseShellExecute = true
            };

            Process.Start(psi);
            return Task.FromResult(ActionResult.Ok($"Opened Git Bash at: {folder}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ActionResult.Fail($"Failed to open Git Bash: {ex.Message}"));
        }
    }

    private static string ResolveFolder(IDictionary<string, object> parameters, ActionContext context)
    {
        if (!parameters.TryGetValue("folder", out var f) || f is not string folder)
            return context.DesktopContext.CurrentFolder;

        return folder == "currentFolder"
            ? context.DesktopContext.CurrentFolder
            : folder;
    }

    private static string? FindGitBash()
    {
        string[] candidates =
        [
            @"C:\Program Files\Git\git-bash.exe",
            @"C:\Program Files (x86)\Git\git-bash.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Git\git-bash.exe")
        ];

        return candidates.FirstOrDefault(File.Exists);
    }
}
