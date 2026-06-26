using Microsoft.EntityFrameworkCore;
using Orbit.Core.Contracts;
using Orbit.Core.Models;
using Orbit.Persistence.Entities;

namespace Orbit.Persistence.Repositories;

public class WorkflowRepository(OrbitDbContext db) : IWorkflowRepository
{
    public async Task<WorkflowRun> SaveRunAsync(WorkflowRun run)
    {
        var entity = ToEntity(run);
        db.WorkflowRuns.Add(entity);
        await db.SaveChangesAsync();
        return run;
    }

    public async Task<WorkflowRun> UpdateRunAsync(WorkflowRun run)
    {
        var entity = await db.WorkflowRuns.FindAsync(run.Id)
            ?? throw new InvalidOperationException($"WorkflowRun {run.Id} not found.");

        entity.Status = run.Status.ToString();
        entity.CompletedAt = run.CompletedAt;
        entity.DurationMs = run.DurationMs;

        await db.SaveChangesAsync();
        return run;
    }

    public async Task<WorkflowStepRun> SaveStepRunAsync(WorkflowStepRun stepRun)
    {
        var entity = ToEntity(stepRun);
        db.WorkflowStepRuns.Add(entity);
        await db.SaveChangesAsync();
        return stepRun;
    }

    public async Task<WorkflowStepRun> UpdateStepRunAsync(WorkflowStepRun stepRun)
    {
        var entity = await db.WorkflowStepRuns.FindAsync(stepRun.Id)
            ?? throw new InvalidOperationException($"WorkflowStepRun {stepRun.Id} not found.");

        entity.Status = stepRun.Status.ToString();
        entity.CompletedAt = stepRun.CompletedAt;
        entity.Message = stepRun.Message;
        entity.Error = stepRun.Error;

        await db.SaveChangesAsync();
        return stepRun;
    }

    public async Task<IReadOnlyList<WorkflowRun>> GetRecentRunsAsync(int count = 50)
    {
        var entities = await db.WorkflowRuns
            .Include(r => r.StepRuns)
            .OrderByDescending(r => r.StartedAt)
            .Take(count)
            .ToListAsync();

        return entities.Select(ToDomain).ToList();
    }

    public async Task<WorkflowRun?> GetRunByIdAsync(Guid id)
    {
        var entity = await db.WorkflowRuns
            .Include(r => r.StepRuns)
            .FirstOrDefaultAsync(r => r.Id == id);

        return entity is null ? null : ToDomain(entity);
    }

    public async Task<IReadOnlyList<WorkflowStepRun>> GetStepRunsForRunAsync(Guid workflowRunId)
    {
        var entities = await db.WorkflowStepRuns
            .Where(s => s.WorkflowRunId == workflowRunId)
            .OrderBy(s => s.StartedAt)
            .ToListAsync();

        return entities.Select(ToDomainStep).ToList();
    }

    private static WorkflowRunEntity ToEntity(WorkflowRun run) => new()
    {
        Id = run.Id,
        WorkflowName = run.WorkflowName,
        Status = run.Status.ToString(),
        StartedAt = run.StartedAt,
        CompletedAt = run.CompletedAt,
        DurationMs = run.DurationMs
    };

    private static WorkflowStepRunEntity ToEntity(WorkflowStepRun step) => new()
    {
        Id = step.Id,
        WorkflowRunId = step.WorkflowRunId,
        StepName = step.StepName,
        Status = step.Status.ToString(),
        StartedAt = step.StartedAt,
        CompletedAt = step.CompletedAt,
        Message = step.Message,
        Error = step.Error
    };

    private static WorkflowRun ToDomain(WorkflowRunEntity entity)
    {
        var run = new WorkflowRun
        {
            Id = entity.Id,
            WorkflowName = entity.WorkflowName,
            Status = Enum.Parse<WorkflowStatus>(entity.Status),
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            DurationMs = entity.DurationMs
        };

        foreach (var step in entity.StepRuns)
            run.StepRuns.Add(ToDomainStep(step));

        return run;
    }

    private static WorkflowStepRun ToDomainStep(WorkflowStepRunEntity entity) => new()
    {
        Id = entity.Id,
        WorkflowRunId = entity.WorkflowRunId,
        StepName = entity.StepName,
        Status = Enum.Parse<WorkflowStatus>(entity.Status),
        StartedAt = entity.StartedAt,
        CompletedAt = entity.CompletedAt,
        Message = entity.Message,
        Error = entity.Error
    };
}
