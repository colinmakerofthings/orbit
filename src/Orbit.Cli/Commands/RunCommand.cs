using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Contracts;
using Orbit.Engine.Engine;
using Spectre.Console;

namespace Orbit.Cli.Commands;

public static class RunCommand
{
    public static async Task<int> ExecuteAsync(IServiceProvider services, string workflowName)
    {
        var engine = services.GetRequiredService<IWorkflowEngine>();

        AnsiConsole.MarkupLine($"[cyan]orbit[/] [grey]→[/] running [bold]{workflowName}[/]");

        var run = await engine.RunAsync(workflowName);

        var statusColor = run.Status switch
        {
            Core.Models.WorkflowStatus.Completed => "green",
            Core.Models.WorkflowStatus.Failed    => "red",
            Core.Models.WorkflowStatus.Cancelled => "yellow",
            _ => "grey"
        };

        AnsiConsole.MarkupLine(
            $"[{statusColor}]{run.Status}[/] in [dim]{run.DurationMs}ms[/]");

        if (run.StepRuns.Any(s => s.Error is not null))
        {
            foreach (var step in run.StepRuns.Where(s => s.Error is not null))
                AnsiConsole.MarkupLine($"  [red]✗[/] {step.StepName}: {Markup.Escape(step.Error!)}");
        }

        return run.Status == Core.Models.WorkflowStatus.Completed ? 0 : 1;
    }
}
