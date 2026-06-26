using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Contracts;
using Orbit.Core.Models;
using Spectre.Console;

namespace Orbit.Cli.Commands;

public static class StatusCommand
{
    public static Task<int> ExecuteAsync(IServiceProvider services)
    {
        var engine = services.GetRequiredService<IWorkflowEngine>();
        var active = engine.GetActiveRuns();

        if (active.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No workflows currently running.[/]");
            return Task.FromResult(0);
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("[bold]Running Workflows[/]")
            .AddColumn("[bold]Workflow[/]")
            .AddColumn("[bold]Step[/]")
            .AddColumn("[bold]Elapsed[/]");

        foreach (var run in active)
        {
            var currentStep = run.StepRuns
                .LastOrDefault(s => s.Status == WorkflowStatus.Running);

            var elapsed = DateTime.UtcNow - run.StartedAt;
            var elapsedStr = elapsed.TotalMinutes >= 1
                ? $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s"
                : $"{elapsed.Seconds}s";

            table.AddRow(
                $"[cyan]{Markup.Escape(run.WorkflowName)}[/]",
                currentStep is not null ? Markup.Escape(currentStep.StepName) : "[grey]starting...[/]",
                elapsedStr);
        }

        AnsiConsole.Write(table);
        return Task.FromResult(0);
    }
}
