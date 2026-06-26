namespace Orbit.Core.Models;

public class OrbitCommand
{
    public required string Name { get; init; }
    public required string WorkflowName { get; init; }
    public string? Description { get; init; }
}
