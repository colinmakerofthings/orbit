using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Contracts;

namespace Orbit.Actions;

public static class ActionsServiceExtensions
{
    public static IServiceCollection AddOrbitActions(this IServiceCollection services)
    {
        services.AddSingleton<IAction, LaunchProcessAction>();
        services.AddSingleton<IAction, KillProcessAction>();
        services.AddSingleton<IAction, WaitAction>();
        services.AddSingleton<IAction, WaitForProcessAction>();
        services.AddSingleton<IAction, TypeTextAction>();
        services.AddSingleton<IAction, TimestampTextAction>();
        services.AddSingleton<IAction, OpenGitBashAction>();
        services.AddSingleton<IAction, OpenBrowserAction>();
        services.AddSingleton<IAction, RunPowerShellAction>();

        return services;
    }
}
