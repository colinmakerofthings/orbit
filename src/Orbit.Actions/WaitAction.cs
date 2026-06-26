using Orbit.Core.Contracts;
using Orbit.Core.Models;

namespace Orbit.Actions;

public class WaitAction : IAction
{
    public string Name => "wait";

    public async Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default)
    {
        int ms = 1000;

        if (parameters.TryGetValue("ms", out var msVal))
            ms = Convert.ToInt32(msVal);
        else if (parameters.TryGetValue("seconds", out var secVal))
            ms = (int)(Convert.ToDouble(secVal) * 1000);

        try
        {
            await Task.Delay(ms, cancellationToken);
            return ActionResult.Ok($"Waited {ms}ms");
        }
        catch (TaskCanceledException)
        {
            return ActionResult.Fail("Wait cancelled");
        }
    }
}
