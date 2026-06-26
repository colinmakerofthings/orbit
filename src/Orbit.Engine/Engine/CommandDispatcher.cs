using Microsoft.Extensions.Logging;
using Orbit.Core.Contracts;

namespace Orbit.Engine.Engine;

public class CommandDispatcher(
    ICommandRegistry registry,
    IWorkflowEngine workflowEngine,
    ILogger<CommandDispatcher> logger)
{
    /// <summary>
    /// Dispatches a command by name. Commands map to a workflow; the workflow is executed.
    /// If no command is registered for the name, tries to run it as a workflow name directly.
    /// </summary>
    public async Task DispatchAsync(string commandName, CancellationToken cancellationToken = default)
    {
        var command = registry.Find(commandName);

        if (command is not null)
        {
            logger.LogInformation("Dispatching command '{Command}' → workflow '{Workflow}'",
                commandName, command.WorkflowName);
            await workflowEngine.RunAsync(command.WorkflowName, cancellationToken);
        }
        else
        {
            // Fall back to treating the name as a direct workflow name
            logger.LogInformation("No command '{Command}' found — running as workflow directly", commandName);
            await workflowEngine.RunAsync(commandName, cancellationToken);
        }
    }
}
