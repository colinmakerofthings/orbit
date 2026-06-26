using Orbit.Core.Models;

namespace Orbit.Core.Contracts;

public interface IWorkflowRepository
{
    Task<WorkflowRun> SaveRunAsync(WorkflowRun run);
    Task<WorkflowRun> UpdateRunAsync(WorkflowRun run);
    Task<WorkflowStepRun> SaveStepRunAsync(WorkflowStepRun stepRun);
    Task<WorkflowStepRun> UpdateStepRunAsync(WorkflowStepRun stepRun);
    Task<IReadOnlyList<WorkflowRun>> GetRecentRunsAsync(int count = 50);
    Task<WorkflowRun?> GetRunByIdAsync(Guid id);
    Task<IReadOnlyList<WorkflowStepRun>> GetStepRunsForRunAsync(Guid workflowRunId);
}
