using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Orbit.Core.Models;
using Orbit.Persistence;
using Orbit.Persistence.Repositories;

namespace Orbit.IntegrationTests;

public class WorkflowRepositoryTests : IDisposable
{
    private readonly OrbitDbContext _db;
    private readonly WorkflowRepository _repo;

    public WorkflowRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<OrbitDbContext>()
            .UseSqlite($"Data Source=:memory:")
            .Options;

        _db = new OrbitDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        _repo = new WorkflowRepository(_db);
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    [Fact]
    public async Task SaveRunAsync_PersistsRun()
    {
        var run = new WorkflowRun
        {
            WorkflowName = "test-workflow",
            Status = WorkflowStatus.Pending
        };

        await _repo.SaveRunAsync(run);

        var stored = await _db.WorkflowRuns.FindAsync(run.Id);
        stored.Should().NotBeNull();
        stored!.WorkflowName.Should().Be("test-workflow");
        stored.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task UpdateRunAsync_UpdatesStatusAndDuration()
    {
        var run = new WorkflowRun { WorkflowName = "wf", Status = WorkflowStatus.Running };
        await _repo.SaveRunAsync(run);

        run.Status = WorkflowStatus.Completed;
        run.CompletedAt = DateTime.UtcNow;
        run.DurationMs = 1234;
        await _repo.UpdateRunAsync(run);

        var stored = await _db.WorkflowRuns.FindAsync(run.Id);
        stored!.Status.Should().Be("Completed");
        stored.DurationMs.Should().Be(1234);
    }

    [Fact]
    public async Task SaveStepRunAsync_PersistsStepRun()
    {
        var run = new WorkflowRun { WorkflowName = "wf", Status = WorkflowStatus.Running };
        await _repo.SaveRunAsync(run);

        var step = new WorkflowStepRun
        {
            WorkflowRunId = run.Id,
            StepName = "launch-process (step 1/2)",
            Status = WorkflowStatus.Running
        };
        await _repo.SaveStepRunAsync(step);

        var stored = await _db.WorkflowStepRuns.FindAsync(step.Id);
        stored.Should().NotBeNull();
        stored!.WorkflowRunId.Should().Be(run.Id);
        stored.StepName.Should().Be("launch-process (step 1/2)");
    }

    [Fact]
    public async Task UpdateStepRunAsync_UpdatesStatusAndMessage()
    {
        var run = new WorkflowRun { WorkflowName = "wf", Status = WorkflowStatus.Running };
        await _repo.SaveRunAsync(run);

        var step = new WorkflowStepRun
        {
            WorkflowRunId = run.Id,
            StepName = "wait",
            Status = WorkflowStatus.Running
        };
        await _repo.SaveStepRunAsync(step);

        step.Status = WorkflowStatus.Completed;
        step.Message = "Waited 500ms";
        step.CompletedAt = DateTime.UtcNow;
        await _repo.UpdateStepRunAsync(step);

        var stored = await _db.WorkflowStepRuns.FindAsync(step.Id);
        stored!.Status.Should().Be("Completed");
        stored.Message.Should().Be("Waited 500ms");
    }

    [Fact]
    public async Task GetRecentRunsAsync_ReturnsInDescendingOrder()
    {
        for (int i = 0; i < 5; i++)
        {
            var run = new WorkflowRun
            {
                WorkflowName = $"wf-{i}",
                Status = WorkflowStatus.Completed,
                StartedAt = DateTime.UtcNow.AddMinutes(-i)
            };
            await _repo.SaveRunAsync(run);
        }

        var runs = await _repo.GetRecentRunsAsync(10);

        runs.Should().HaveCount(5);
        runs.Should().BeInDescendingOrder(r => r.StartedAt);
    }

    [Fact]
    public async Task GetRecentRunsAsync_RespectsCountLimit()
    {
        for (int i = 0; i < 10; i++)
            await _repo.SaveRunAsync(new WorkflowRun { WorkflowName = "wf", Status = WorkflowStatus.Completed });

        var runs = await _repo.GetRecentRunsAsync(3);
        runs.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetRunByIdAsync_ReturnsRunWithSteps()
    {
        var run = new WorkflowRun { WorkflowName = "wf", Status = WorkflowStatus.Completed };
        await _repo.SaveRunAsync(run);

        var step1 = new WorkflowStepRun { WorkflowRunId = run.Id, StepName = "s1", Status = WorkflowStatus.Completed };
        var step2 = new WorkflowStepRun { WorkflowRunId = run.Id, StepName = "s2", Status = WorkflowStatus.Completed };
        await _repo.SaveStepRunAsync(step1);
        await _repo.SaveStepRunAsync(step2);

        var fetched = await _repo.GetRunByIdAsync(run.Id);

        fetched.Should().NotBeNull();
        fetched!.StepRuns.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRunByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _repo.GetRunByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetStepRunsForRunAsync_ReturnsInChronologicalOrder()
    {
        var run = new WorkflowRun { WorkflowName = "wf", Status = WorkflowStatus.Completed };
        await _repo.SaveRunAsync(run);

        var step1 = new WorkflowStepRun { WorkflowRunId = run.Id, StepName = "first",  Status = WorkflowStatus.Completed, StartedAt = DateTime.UtcNow.AddSeconds(-2) };
        var step2 = new WorkflowStepRun { WorkflowRunId = run.Id, StepName = "second", Status = WorkflowStatus.Completed, StartedAt = DateTime.UtcNow.AddSeconds(-1) };
        await _repo.SaveStepRunAsync(step1);
        await _repo.SaveStepRunAsync(step2);

        var steps = await _repo.GetStepRunsForRunAsync(run.Id);

        steps.Should().HaveCount(2);
        steps[0].StepName.Should().Be("first");
        steps[1].StepName.Should().Be("second");
    }

    [Fact]
    public async Task DeleteRun_CascadesStepRuns()
    {
        var run = new WorkflowRun { WorkflowName = "wf", Status = WorkflowStatus.Completed };
        await _repo.SaveRunAsync(run);
        await _repo.SaveStepRunAsync(new WorkflowStepRun { WorkflowRunId = run.Id, StepName = "s1", Status = WorkflowStatus.Completed });

        var entity = await _db.WorkflowRuns.FindAsync(run.Id);
        _db.WorkflowRuns.Remove(entity!);
        await _db.SaveChangesAsync();

        var steps = await _db.WorkflowStepRuns.Where(s => s.WorkflowRunId == run.Id).ToListAsync();
        steps.Should().BeEmpty();
    }
}
