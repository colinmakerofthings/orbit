using Microsoft.Extensions.DependencyInjection;
using Orbit.Cli;
using Orbit.Cli.Commands;
using Spectre.Console;

if (args.Length == 0)
{
    PrintHelp();
    return 0;
}

var services = ServiceSetup.Build();
var command = args[0].ToLowerInvariant();

return command switch
{
    "run"       => await RunCommand.ExecuteAsync(services, RequireArg(args, 1, "workflow name")),
    "start"     => await RunCommand.ExecuteAsync(services, RequireArg(args, 1, "workflow name")),
    "stop"      => await StopCommand(services, RequireArg(args, 1, "workflow name")),
    "workflows" => await WorkflowsCommand.ExecuteAsync(services),
    "history"   => await HistoryCommand.ExecuteAsync(services),
    "status"    => await StatusCommand.ExecuteAsync(services),
    "logs"      => await LogsCommand.ExecuteAsync(services, RequireArg(args, 1, "workflow name")),
    "help" or "--help" or "-h" => Help(),
    _ => Unknown(command)
};

static string RequireArg(string[] args, int index, string name)
{
    if (args.Length <= index || string.IsNullOrWhiteSpace(args[index]))
    {
        AnsiConsole.MarkupLine($"[red]Error:[/] missing {name}");
        Environment.Exit(1);
    }
    return args[index];
}

static async Task<int> StopCommand(IServiceProvider services, string workflowName)
{
    var engine = services.GetRequiredService<Orbit.Core.Contracts.IWorkflowEngine>();
    await engine.CancelAsync(workflowName);
    AnsiConsole.MarkupLine($"[yellow]Cancel requested:[/] {workflowName}");
    return 0;
}

static int Help()
{
    PrintHelp();
    return 0;
}

static int Unknown(string cmd)
{
    AnsiConsole.MarkupLine($"[red]Unknown command:[/] {cmd}");
    AnsiConsole.MarkupLine("Run [bold]orbit help[/] for usage.");
    return 1;
}

static void PrintHelp()
{
    AnsiConsole.MarkupLine("[bold cyan]orbit[/] — desktop automation engine\n");
    AnsiConsole.MarkupLine("[bold]Usage:[/]  orbit <command> [[args]]\n");

    var table = new Spectre.Console.Table()
        .HideHeaders()
        .Border(Spectre.Console.TableBorder.None)
        .AddColumn("")
        .AddColumn("");

    table.AddRow("[cyan]orbit run <workflow>[/]",       "Run a workflow by name");
    table.AddRow("[cyan]orbit start <workflow>[/]",     "Alias for run");
    table.AddRow("[cyan]orbit stop <workflow>[/]",      "Cancel a running workflow");
    table.AddRow("[cyan]orbit workflows[/]",            "List all available workflows");
    table.AddRow("[cyan]orbit history[/]",              "Show recent workflow run history");
    table.AddRow("[cyan]orbit status[/]",               "Show currently running workflows");
    table.AddRow("[cyan]orbit logs <workflow>[/]",      "Show step logs for last run of a workflow");

    AnsiConsole.Write(table);
}
