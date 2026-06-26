using Microsoft.Extensions.Logging;
using Orbit.Core.Contracts;
using Orbit.Core.Models;

namespace Orbit.Engine.Engine;

public class WorkflowEngine(
    WorkflowLoader loader,
    IEnumerable<IAction> actions,
    IWorkflowRepository repository,
    IContextProvider contextProvider,
    ILogger<WorkflowEngine> logger) : IWorkflowEngine
{
    private readonly Dictionary<string, IAction> _actions =
        actions.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<Guid, WorkflowRun> _activeRuns = new();

    public async Task<WorkflowRun> RunAsync(string workflowName, CancellationToken cancellationToken = default)
    {
        var workflow = loader.Load(workflowName)
            ?? throw new InvalidOperationException($"Workflow not found: '{workflowName}'");

        var run = new WorkflowRun
        {
            WorkflowName = workflowName,
            Status = WorkflowStatus.Pending
        };

        await repository.SaveRunAsync(run);
        _activeRuns[run.Id] = run;

        logger.LogInformation("Starting workflow '{Workflow}' (RunId: {RunId})", workflowName, run.Id);

        run.Status = WorkflowStatus.Running;
        await repository.UpdateRunAsync(run);

        var context = await contextProvider.GetCurrentContextAsync();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            for (var i = 0; i < workflow.Steps.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var step = workflow.Steps[i];
                var stepName = $"{step.Action} (step {i + 1}/{workflow.Steps.Count})";

                var stepRun = new WorkflowStepRun
                {
                    WorkflowRunId = run.Id,
                    StepName = stepName,
                    Status = WorkflowStatus.Pending
                };

                await repository.SaveStepRunAsync(stepRun);
                run.StepRuns.Add(stepRun);

                logger.LogInformation("  Step {Index}/{Total}: {Action}", i + 1, workflow.Steps.Count, step.Action);

                stepRun.Status = WorkflowStatus.Running;
                await repository.UpdateStepRunAsync(stepRun);

                if (!_actions.TryGetValue(step.Action, out var action))
                {
                    stepRun.Status = WorkflowStatus.Failed;
                    stepRun.Error = $"Unknown action: '{step.Action}'";
                    stepRun.CompletedAt = DateTime.UtcNow;
                    await repository.UpdateStepRunAsync(stepRun);

                    throw new InvalidOperationException($"Unknown action: '{step.Action}'");
                }

                var actionContext = new ActionContext
                {
                    WorkflowName = workflowName,
                    StepName = step.Action,
                    DesktopContext = context,
                    CancellationToken = cancellationToken
                };

                var result = await action.Execute(actionContext, step.Parameters, cancellationToken);

                stepRun.Status = result.Success ? WorkflowStatus.Completed : WorkflowStatus.Failed;
                stepRun.Message = result.Message;
                stepRun.Error = result.Error;
                stepRun.CompletedAt = DateTime.UtcNow;
                await repository.UpdateStepRunAsync(stepRun);

                if (!result.Success)
                {
                    logger.LogError("  Step failed: {Error}", result.Error);
                    throw new InvalidOperationException($"Step '{step.Action}' failed: {result.Error}");
                }

                logger.LogInformation("  Step completed: {Message}", result.Message);
            }

            sw.Stop();
            run.Status = WorkflowStatus.Completed;
            run.CompletedAt = DateTime.UtcNow;
            run.DurationMs = sw.ElapsedMilliseconds;
            await repository.UpdateRunAsync(run);

            logger.LogInformation("Workflow '{Workflow}' completed in {Duration}ms", workflowName, sw.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            run.Status = WorkflowStatus.Cancelled;
            run.CompletedAt = DateTime.UtcNow;
            run.DurationMs = sw.ElapsedMilliseconds;
            await repository.UpdateRunAsync(run);
            logger.LogWarning("Workflow '{Workflow}' was cancelled", workflowName);
        }
        catch (Exception ex)
        {
            sw.Stop();
            run.Status = WorkflowStatus.Failed;
            run.CompletedAt = DateTime.UtcNow;
            run.DurationMs = sw.ElapsedMilliseconds;
            await repository.UpdateRunAsync(run);
            logger.LogError(ex, "Workflow '{Workflow}' failed", workflowName);
        }
        finally
        {
            _activeRuns.Remove(run.Id);
        }

        return run;
    }

    public Task<WorkflowRun?> GetRunningWorkflowAsync(string workflowName)
    {
        var run = _activeRuns.Values
            .FirstOrDefault(r => r.WorkflowName.Equals(workflowName, StringComparison.OrdinalIgnoreCase)
                               && r.Status == WorkflowStatus.Running);
        return Task.FromResult(run);
    }

    public Task CancelAsync(string workflowName)
    {
        // Cancellation is handled via CancellationToken passed to RunAsync
        logger.LogInformation("Cancel requested for workflow '{Workflow}'", workflowName);
        return Task.CompletedTask;
    }

    public IReadOnlyList<WorkflowRun> GetActiveRuns() => _activeRuns.Values.ToList();
}
