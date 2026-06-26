using FluentAssertions;
using Orbit.Actions;
using Orbit.Core.Models;

namespace Orbit.UnitTests;

public class ActionTests
{
    private static ActionContext MakeContext() => new()
    {
        WorkflowName = "test",
        StepName = "test-step",
        DesktopContext = new ContextData
        {
            ActiveApplication = "explorer.exe",
            CurrentFolder = @"C:\Projects",
            WindowTitle = "Test"
        }
    };

    [Fact]
    public async Task WaitAction_WaitsSpecifiedMs()
    {
        var action = new WaitAction();
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = await action.Execute(MakeContext(),
            new Dictionary<string, object> { ["ms"] = "200" });

        sw.Stop();
        result.Success.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(150);
    }

    [Fact]
    public async Task WaitAction_SupportsSecondsParam()
    {
        var action = new WaitAction();
        var result = await action.Execute(MakeContext(),
            new Dictionary<string, object> { ["seconds"] = "0.1" });

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("ms");
    }

    [Fact]
    public async Task WaitAction_Cancelled_ReturnsFail()
    {
        var action = new WaitAction();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await action.Execute(MakeContext(),
            new Dictionary<string, object> { ["ms"] = "5000" },
            cts.Token);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("cancel");
    }

    [Fact]
    public async Task KillProcessAction_MissingParam_ReturnsFail()
    {
        var action = new KillProcessAction();
        var result = await action.Execute(MakeContext(), new Dictionary<string, object>());

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("process");
    }

    [Fact]
    public async Task LaunchProcessAction_MissingParam_ReturnsFail()
    {
        var action = new LaunchProcessAction();
        var result = await action.Execute(MakeContext(), new Dictionary<string, object>());

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("executable");
    }

    [Fact]
    public async Task TimestampTextAction_DefaultTemplate_Succeeds()
    {
        // We can't verify the actual keystrokes in a unit test, but we can verify
        // the action doesn't throw and returns a success with a timestamp message
        var action = new TimestampTextAction();

        // Use a no-op template with no characters to avoid actually injecting keystrokes
        var result = await action.Execute(MakeContext(),
            new Dictionary<string, object> { ["template"] = "" });

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task OpenBrowserAction_MissingUrl_ReturnsFail()
    {
        var action = new OpenBrowserAction();
        var result = await action.Execute(MakeContext(), new Dictionary<string, object>());

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("url");
    }

    [Fact]
    public async Task WaitForProcessAction_MissingParam_ReturnsFail()
    {
        var action = new WaitForProcessAction();
        var result = await action.Execute(MakeContext(), new Dictionary<string, object>());

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("process");
    }
}
