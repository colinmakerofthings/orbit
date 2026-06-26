namespace Orbit.Core.Models;

public class Workflow
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public IList<WorkflowStep> Steps { get; init; } = new List<WorkflowStep>();
}
