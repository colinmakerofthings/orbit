using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Orbit.Core.Contracts;
using Orbit.Core.Models;
using Orbit.Engine.Engine;

namespace Orbit.UnitTests;

public class WorkflowEngineTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly Mock<IWorkflowRepository> _repoMock = new();
    private readonly Mock<IContextProvider> _contextMock = new();

    public WorkflowEngineTests()
    {
        Directory.CreateDirectory(_tempDir);

        _repoMock.Setup(r => r.SaveRunAsync(It.IsAny<WorkflowRun>()))
                 .ReturnsAsync((WorkflowRun r) => r);
        _repoMock.Setup(r => r.UpdateRunAsync(It.IsAny<WorkflowRun>()))
                 .ReturnsAsync((WorkflowRun r) => r);
        _repoMock.Setup(r => r.SaveStepRunAsync(It.IsAny<WorkflowStepRun>()))
                 .ReturnsAsync((WorkflowStepRun s) => s);
        _repoMock.Setup(r => r.UpdateStepRunAsync(It.IsAny<WorkflowStepRun>()))
                 .ReturnsAsync((WorkflowStepRun s) => s);

        _contextMock.Setup(c => c.GetCurrentContextAsync())
                    .ReturnsAsync(new ContextData
                    {
                        ActiveApplication = "explorer.exe",
                        CurrentFolder = @"C:\Projects",
                        WindowTitle = "Test"
                    });
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public async Task RunAsync_SuccessfulWorkflow_ReturnsCompletedRun()
    {
        WriteYaml("test.yaml", """
            name: test
            steps:
              - action: succeed
            """);

        var action = MakeAction("succeed", ActionResult.Ok("done"));
        var engine = BuildEngine([action]);

        var run = await engine.RunAsync("test");

        run.Status.Should().Be(WorkflowStatus.Completed);
        run.DurationMs.Should().BeGreaterThanOrEqualTo(0);
        run.StepRuns.Should().HaveCount(1);
        run.StepRuns[0].Status.Should().Be(WorkflowStatus.Completed);
    }

    [Fact]
    public async Task RunAsync_ActionFails_ReturnsFailedRun()
    {
        WriteYaml("failing.yaml", """
            name: failing
            steps:
              - action: fail
            """);

        var action = MakeAction("fail", ActionResult.Fail("something went wrong"));
        var engine = BuildEngine([action]);

        var run = await engine.RunAsync("failing");

        run.Status.Should().Be(WorkflowStatus.Failed);
        run.StepRuns[0].Status.Should().Be(WorkflowStatus.Failed);
        run.StepRuns[0].Error.Should().Be("something went wrong");
    }

    [Fact]
    public async Task RunAsync_UnknownWorkflow_ThrowsInvalidOperation()
    {
        var engine = BuildEngine([]);
        var act = async () => await engine.RunAsync("no-such-workflow");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task RunAsync_UnknownAction_RunFailsWithError()
    {
        WriteYaml("unknown-action.yaml", """
            name: unknown-action
            steps:
              - action: does-not-exist
            """);

        var engine = BuildEngine([]);
        var run = await engine.RunAsync("unknown-action");

        run.Status.Should().Be(WorkflowStatus.Failed);
        run.StepRuns[0].Error.Should().Contain("does-not-exist");
    }

    [Fact]
    public async Task RunAsync_MultipleSteps_ExecutesAllInOrder()
    {
        WriteYaml("multi.yaml", """
            name: multi
            steps:
              - action: step1
              - action: step2
              - action: step3
            """);

        var order = new List<string>();
        var actions = new[] { "step1", "step2", "step3" }
            .Select(name =>
            {
                var mock = new Mock<IAction>();
                mock.Setup(a => a.Name).Returns(name);
                mock.Setup(a => a.Execute(It.IsAny<ActionContext>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => { order.Add(name); return ActionResult.Ok(); });
                return mock.Object;
            }).ToArray();

        var engine = BuildEngine(actions);
        var run = await engine.RunAsync("multi");

        run.Status.Should().Be(WorkflowStatus.Completed);
        order.Should().Equal("step1", "step2", "step3");
    }

    [Fact]
    public async Task RunAsync_Cancelled_ReturnscancelledRun()
    {
        WriteYaml("slow.yaml", """
            name: slow
            steps:
              - action: slow-step
            """);

        var cts = new CancellationTokenSource();
        var action = new Mock<IAction>();
        action.Setup(a => a.Name).Returns("slow-step");
        action.Setup(a => a.Execute(It.IsAny<ActionContext>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
              .Returns(async (ActionContext _, IDictionary<string, object> _, CancellationToken ct) =>
              {
                  await Task.Delay(5000, ct);
                  return ActionResult.Ok();
              });

        var engine = BuildEngine([action.Object]);
        cts.CancelAfter(100);

        var run = await engine.RunAsync("slow", cts.Token);
        run.Status.Should().Be(WorkflowStatus.Cancelled);
    }

    [Fact]
    public void GetActiveRuns_NoRunning_ReturnsEmpty()
    {
        var engine = BuildEngine([]);
        engine.GetActiveRuns().Should().BeEmpty();
    }

    private WorkflowEngine BuildEngine(IAction[] actions) => new(
        new WorkflowLoader(_tempDir),
        actions,
        _repoMock.Object,
        _contextMock.Object,
        NullLogger<WorkflowEngine>.Instance);

    private static IAction MakeAction(string name, ActionResult result)
    {
        var mock = new Mock<IAction>();
        mock.Setup(a => a.Name).Returns(name);
        mock.Setup(a => a.Execute(It.IsAny<ActionContext>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return mock.Object;
    }

    private void WriteYaml(string fileName, string content) =>
        File.WriteAllText(Path.Combine(_tempDir, fileName), content);
}
