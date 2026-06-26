namespace Orbit.Persistence.Entities;

public class WorkflowRunEntity
{
    public Guid Id { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? DurationMs { get; set; }

    public ICollection<WorkflowStepRunEntity> StepRuns { get; set; } = new List<WorkflowStepRunEntity>();
}
