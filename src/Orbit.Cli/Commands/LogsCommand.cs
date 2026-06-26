using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Contracts;
using Orbit.Core.Models;
using Spectre.Console;

namespace Orbit.Cli.Commands;

public static class LogsCommand
{
    public static async Task<int> ExecuteAsync(IServiceProvider services, string workflowName)
    {
        var repo = services.GetRequiredService<IWorkflowRepository>();
        var runs = await repo.GetRecentRunsAsync(50);

        var run = runs.FirstOrDefault(r =>
            r.WorkflowName.Equals(workflowName, StringComparison.OrdinalIgnoreCase));

        if (run is null)
        {
            AnsiConsole.MarkupLine($"[yellow]No history found for workflow:[/] {Markup.Escape(workflowName)}");
            return 1;
        }

        var statusColor = run.Status switch
        {
            WorkflowStatus.Completed => "green",
            WorkflowStatus.Failed    => "red",
            WorkflowStatus.Cancelled => "yellow",
            _ => "grey"
        };

        AnsiConsole.MarkupLine(
            $"[bold]{Markup.Escape(run.WorkflowName)}[/]  " +
            $"[{statusColor}]{run.Status}[/]  " +
            $"[dim]{run.StartedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss}  {run.DurationMs}ms[/]");

        AnsiConsole.WriteLine();

        var steps = await repo.GetStepRunsForRunAsync(run.Id);

        foreach (var step in steps)
        {
            var icon = step.Status switch
            {
                WorkflowStatus.Completed => "[green]✓[/]",
                WorkflowStatus.Failed    => "[red]✗[/]",
                WorkflowStatus.Cancelled => "[yellow]○[/]",
                WorkflowStatus.Running   => "[cyan]►[/]",
                _ => "[grey]·[/]"
            };

            var duration = step.CompletedAt.HasValue
                ? $"[dim]{(int)(step.CompletedAt.Value - step.StartedAt).TotalMilliseconds}ms[/]"
                : string.Empty;

            AnsiConsole.MarkupLine($"  {icon} {Markup.Escape(step.StepName)}  {duration}");

            if (step.Message is not null)
                AnsiConsole.MarkupLine($"      [dim]{Markup.Escape(step.Message)}[/]");

            if (step.Error is not null)
                AnsiConsole.MarkupLine($"      [red]{Markup.Escape(step.Error)}[/]");
        }

        return 0;
    }
}
