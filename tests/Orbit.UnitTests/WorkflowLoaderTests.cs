using FluentAssertions;
using Orbit.Core.Models;
using Orbit.Engine.Engine;

namespace Orbit.UnitTests;

public class WorkflowLoaderTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public WorkflowLoaderTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void Load_ValidWorkflow_ReturnsWorkflow()
    {
        WriteYaml("greet.yaml", """
            name: greet
            description: A test workflow
            steps:
              - action: wait
                ms: 100
            """);

        var loader = new WorkflowLoader(_tempDir);
        var workflow = loader.Load("greet");

        workflow.Should().NotBeNull();
        workflow!.Name.Should().Be("greet");
        workflow.Description.Should().Be("A test workflow");
        workflow.Steps.Should().HaveCount(1);
        workflow.Steps[0].Action.Should().Be("wait");
        workflow.Steps[0].Parameters["ms"].Should().Be("100");
    }

    [Fact]
    public void Load_MultipleStepsWithParams_ParsesAllParameters()
    {
        WriteYaml("multi.yaml", """
            name: multi
            steps:
              - action: launch-process
                executable: notepad.exe
                arguments: /w
              - action: wait
                ms: 500
            """);

        var loader = new WorkflowLoader(_tempDir);
        var workflow = loader.Load("multi");

        workflow!.Steps.Should().HaveCount(2);
        workflow.Steps[0].Action.Should().Be("launch-process");
        workflow.Steps[0].Parameters["executable"].Should().Be("notepad.exe");
        workflow.Steps[0].Parameters["arguments"].Should().Be("/w");
        workflow.Steps[1].Action.Should().Be("wait");
    }

    [Fact]
    public void Load_NonExistentWorkflow_ReturnsNull()
    {
        var loader = new WorkflowLoader(_tempDir);
        loader.Load("does-not-exist").Should().BeNull();
    }

    [Fact]
    public void LoadAll_MultipleFiles_ReturnsAll()
    {
        WriteYaml("a.yaml", "name: a\nsteps:\n  - action: wait\n    ms: 1");
        WriteYaml("b.yaml", "name: b\nsteps:\n  - action: wait\n    ms: 2");

        var loader = new WorkflowLoader(_tempDir);
        var all = loader.LoadAll();

        all.Should().HaveCount(2);
        all.Select(w => w.Name).Should().Contain(["a", "b"]);
    }

    [Fact]
    public void Load_StepsMissingActionKey_ThrowsInvalidOperation()
    {
        WriteYaml("bad.yaml", """
            name: bad
            steps:
              - executable: notepad.exe
            """);

        var loader = new WorkflowLoader(_tempDir);
        var act = () => loader.Load("bad");
        act.Should().Throw<InvalidOperationException>().WithMessage("*action*");
    }

    private void WriteYaml(string fileName, string content) =>
        File.WriteAllText(Path.Combine(_tempDir, fileName), content);
}
