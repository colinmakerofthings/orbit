namespace Orbit.Core.Models;

public class WorkflowStep
{
    public required string Action { get; init; }
    public IDictionary<string, object> Parameters { get; init; } = new Dictionary<string, object>();
}
