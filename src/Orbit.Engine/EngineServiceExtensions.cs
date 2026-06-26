using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Contracts;
using Orbit.Engine.Engine;
using Orbit.Engine.Infrastructure;

namespace Orbit.Engine;

public static class EngineServiceExtensions
{
    public static IServiceCollection AddOrbitEngine(
        this IServiceCollection services,
        string workflowsDirectory)
    {
        services.AddSingleton(new WorkflowLoader(workflowsDirectory));
        services.AddSingleton<ICommandRegistry, CommandRegistry>();
        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
        services.AddSingleton<IContextProvider, WindowsContextProvider>();
        services.AddSingleton<IHotkeyService, WindowsHotkeyService>();
        services.AddSingleton<CommandDispatcher>();

        return services;
    }
}
