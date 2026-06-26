using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orbit.Actions;
using Orbit.Engine;
using Orbit.Persistence;

namespace Orbit.Cli;

public static class ServiceSetup
{
    public static IServiceProvider Build()
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Orbit", "orbit.db");

        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        var workflowsDir = ResolveWorkflowsDirectory();

        var services = new ServiceCollection();

        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddOrbitPersistence(dbPath);
        services.AddOrbitActions();
        services.AddOrbitEngine(workflowsDir);

        var provider = services.BuildServiceProvider();

        // Apply migrations
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrbitDbContext>();
        db.Database.EnsureCreated();

        return provider;
    }

    private static string ResolveWorkflowsDirectory()
    {
        // Walk up from the executable looking for a /workflows folder
        var dir = AppContext.BaseDirectory;
        for (int i = 0; i < 6; i++)
        {
            var candidate = Path.Combine(dir, "workflows");
            if (Directory.Exists(candidate))
                return candidate;
            dir = Path.GetDirectoryName(dir) ?? dir;
        }

        // Default: %LOCALAPPDATA%\Orbit\workflows
        var fallback = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Orbit", "workflows");
        Directory.CreateDirectory(fallback);
        return fallback;
    }
}
