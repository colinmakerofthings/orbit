namespace Orbit.Core.Models;

public class ActionContext
{
    public required string WorkflowName { get; init; }
    public required string StepName { get; init; }
    public required ContextData DesktopContext { get; init; }
    public CancellationToken CancellationToken { get; init; }
}
