using Orbit.Core.Contracts;
using Orbit.Core.Models;
using System.Diagnostics;

namespace Orbit.Actions;

public class WaitForProcessAction : IAction
{
    public string Name => "wait-for-process";

    public async Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        if (!parameters.TryGetValue("process", out var proc) || proc is not string processName)
            return ActionResult.Fail("Missing required parameter: process");

        var timeoutMs = parameters.TryGetValue("timeoutSeconds", out var t)
            ? (int)(Convert.ToDouble(t) * 1000)
            : 120_000;

        var pollMs = 2000;
        var name = Path.GetFileNameWithoutExtension(processName);
        var elapsed = 0;

        while (elapsed < timeoutMs)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Process.GetProcessesByName(name).Length > 0)
                return ActionResult.Ok($"Process ready: {processName}");

            await Task.Delay(pollMs, cancellationToken);
            elapsed += pollMs;
        }

        return ActionResult.Fail($"Timed out waiting for process: {processName}");
    }
}
