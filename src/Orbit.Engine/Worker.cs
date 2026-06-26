using Orbit.Core.Contracts;
using Orbit.Engine.Engine;

namespace Orbit.Engine;

/// <summary>
/// Main background service. Loads hotkey config, registers hotkeys, and keeps the engine alive.
/// </summary>
public class Worker(
    IHotkeyService hotkeyService,
    IWorkflowEngine workflowEngine,
    ILogger<Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Walk up from the binary to find orbit.yaml in the repo root
        var configPath = ResolveConfigPath();

        var hotkeys = HotkeyConfigLoader.Load(configPath);

        if (hotkeys.Count == 0)
            logger.LogWarning("No hotkeys configured (looked for orbit.yaml at {Path})", configPath);

        foreach (var (hotkey, workflowName) in hotkeys)
        {
            var name = workflowName; // capture for lambda
            hotkeyService.Register(hotkey, async () =>
            {
                // Brief delay so hotkey modifier keys (Ctrl, Alt, etc.) are released
                // before SendInput fires — otherwise they'd be sent as modified characters.
                await Task.Delay(300);
                logger.LogInformation("Hotkey {Hotkey} triggered → running '{Workflow}'", hotkey, name);
                try
                {
                    await workflowEngine.RunAsync(name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Workflow '{Workflow}' failed", name);
                }
            });
        }

        logger.LogInformation("Orbit Engine started ({Count} hotkeys registered)", hotkeys.Count);
        hotkeyService.Start();

        stoppingToken.Register(() =>
        {
            logger.LogInformation("Orbit Engine stopping");
            hotkeyService.Stop();
        });

        // Keep alive — hotkey service runs its own message loop thread
        return Task.Delay(Timeout.Infinite, stoppingToken)
            .ContinueWith(_ => { }, TaskContinuationOptions.None);
    }

    private static string ResolveConfigPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "orbit.yaml");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        return Path.Combine(AppContext.BaseDirectory, "orbit.yaml");
    }
}
