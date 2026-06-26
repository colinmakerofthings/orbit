using Orbit.Core.Contracts;
using Orbit.Core.Models;
using System.Diagnostics;

namespace Orbit.Actions;

public class OpenBrowserAction : IAction
{
    public string Name => "open-browser";

    public Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        if (!parameters.TryGetValue("url", out var u) || u is not string url)
            return Task.FromResult(ActionResult.Fail("Missing required parameter: url"));

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return Task.FromResult(ActionResult.Ok($"Opened: {url}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ActionResult.Fail($"Failed to open browser: {ex.Message}"));
        }
    }
}
