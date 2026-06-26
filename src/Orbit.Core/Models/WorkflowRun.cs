namespace Orbit.Core.Models;

public class WorkflowRun
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string WorkflowName { get; init; }
    public WorkflowStatus Status { get; set; }
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }
    public IList<WorkflowStepRun> StepRuns { get; init; } = new List<WorkflowStepRun>();
}
