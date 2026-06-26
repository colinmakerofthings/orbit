using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Contracts;
using Orbit.Core.Models;
using Spectre.Console;

namespace Orbit.Cli.Commands;

public static class HistoryCommand
{
    public static async Task<int> ExecuteAsync(IServiceProvider services, int count = 20)
    {
        var repo = services.GetRequiredService<IWorkflowRepository>();
        var runs = await repo.GetRecentRunsAsync(count);

        if (runs.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No workflow history found.[/]");
            return 0;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Started[/]")
            .AddColumn("[bold]Workflow[/]")
            .AddColumn("[bold]Status[/]")
            .AddColumn("[bold]Duration[/]");

        foreach (var run in runs)
        {
            var statusMarkup = run.Status switch
            {
                WorkflowStatus.Completed => "[green]Completed[/]",
                WorkflowStatus.Failed    => "[red]Failed[/]",
                WorkflowStatus.Cancelled => "[yellow]Cancelled[/]",
                WorkflowStatus.Running   => "[cyan]Running[/]",
                _                        => run.Status.ToString()
            };

            table.AddRow(
                run.StartedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                Markup.Escape(run.WorkflowName),
                statusMarkup,
                run.DurationMs.HasValue ? $"{run.DurationMs}ms" : "-");
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
