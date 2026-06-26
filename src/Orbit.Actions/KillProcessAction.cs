using Orbit.Core.Contracts;
using Orbit.Core.Models;
using System.Diagnostics;

namespace Orbit.Actions;

public class KillProcessAction : IAction
{
    public string Name => "kill-process";

    public Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        if (!parameters.TryGetValue("process", out var proc) || proc is not string processName)
            return Task.FromResult(ActionResult.Fail("Missing required parameter: process"));

        try
        {
            var processes = Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(processName));

            if (processes.Length == 0)
                return Task.FromResult(ActionResult.Ok($"No running process found: {processName}"));

            foreach (var p in processes)
            {
                p.Kill(entireProcessTree: true);
                p.Dispose();
            }

            return Task.FromResult(ActionResult.Ok($"Killed {processes.Length} process(es): {processName}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ActionResult.Fail($"Failed to kill '{processName}': {ex.Message}"));
        }
    }
}
