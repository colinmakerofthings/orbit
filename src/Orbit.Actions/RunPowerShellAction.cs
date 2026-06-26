using Orbit.Core.Contracts;
using Orbit.Core.Models;
using System.Diagnostics;
using System.Text;

namespace Orbit.Actions;

public class RunPowerShellAction : IAction
{
    public string Name => "run-powershell";

    public async Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        if (!parameters.TryGetValue("script", out var s) || s is not string script)
            return ActionResult.Fail("Missing required parameter: script");

        var workingDir = parameters.TryGetValue("workingDirectory", out var wd) && wd is string wdStr
            ? wdStr
            : context.DesktopContext.CurrentFolder;

        try
        {
            var output = new StringBuilder();
            var error = new StringBuilder();

            var psi = new ProcessStartInfo
            {
                FileName = "pwsh.exe",
                Arguments = $"-NoProfile -NonInteractive -Command \"{script.Replace("\"", "\\\"")}\"",
                WorkingDirectory = Directory.Exists(workingDir) ? workingDir : null,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) error.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                return ActionResult.Fail($"Script exited with code {process.ExitCode}: {error}");

            return ActionResult.Ok(output.ToString().TrimEnd());
        }
        catch (Exception ex)
        {
            return ActionResult.Fail($"Failed to run PowerShell: {ex.Message}");
        }
    }
}
