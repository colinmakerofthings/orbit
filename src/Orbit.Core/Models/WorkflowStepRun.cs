namespace Orbit.Core.Models;

public class WorkflowStepRun
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid WorkflowRunId { get; init; }
    public required string StepName { get; init; }
    public WorkflowStatus Status { get; set; }
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}
