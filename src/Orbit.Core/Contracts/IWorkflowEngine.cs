using Orbit.Core.Models;

namespace Orbit.Core.Contracts;

public interface IWorkflowEngine
{
    Task<WorkflowRun> RunAsync(string workflowName, CancellationToken cancellationToken = default);
    Task<WorkflowRun?> GetRunningWorkflowAsync(string workflowName);
    Task CancelAsync(string workflowName);
    IReadOnlyList<WorkflowRun> GetActiveRuns();
}
