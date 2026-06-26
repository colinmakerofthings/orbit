using Microsoft.Extensions.DependencyInjection;
using Orbit.Engine.Engine;
using Spectre.Console;

namespace Orbit.Cli.Commands;

public static class WorkflowsCommand
{
    public static Task<int> ExecuteAsync(IServiceProvider services)
    {
        var loader = services.GetRequiredService<WorkflowLoader>();
        var workflows = loader.LoadAll();

        if (workflows.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No workflows found.[/]");
            return Task.FromResult(0);
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("[bold]Name[/]")
            .AddColumn("[bold]Steps[/]")
            .AddColumn("[bold]Description[/]");

        foreach (var wf in workflows.OrderBy(w => w.Name))
        {
            table.AddRow(
                $"[cyan]{Markup.Escape(wf.Name)}[/]",
                wf.Steps.Count.ToString(),
                Markup.Escape(wf.Description ?? ""));
        }

        AnsiConsole.Write(table);
        return Task.FromResult(0);
    }
}
