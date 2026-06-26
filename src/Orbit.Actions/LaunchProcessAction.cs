using Orbit.Core.Contracts;
using Orbit.Core.Models;
using System.Diagnostics;

namespace Orbit.Actions;

public class LaunchProcessAction : IAction
{
    public string Name => "launch-process";

    public Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        if (!parameters.TryGetValue("executable", out var exe) || exe is not string executable)
            return Task.FromResult(ActionResult.Fail("Missing required parameter: executable"));

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = executable,
                UseShellExecute = true
            };

            if (parameters.TryGetValue("arguments", out var args) && args is string arguments)
                psi.Arguments = arguments;

            if (parameters.TryGetValue("workingDirectory", out var wd) && wd is string workingDir)
                psi.WorkingDirectory = workingDir;

            Process.Start(psi);
            return Task.FromResult(ActionResult.Ok($"Launched: {executable}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ActionResult.Fail($"Failed to launch '{executable}': {ex.Message}"));
        }
    }
}
