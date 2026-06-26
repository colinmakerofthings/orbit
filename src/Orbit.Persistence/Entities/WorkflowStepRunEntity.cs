namespace Orbit.Persistence.Entities;

public class WorkflowStepRunEntity
{
    public Guid Id { get; set; }
    public Guid WorkflowRunId { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }

    public WorkflowRunEntity WorkflowRun { get; set; } = null!;
}
