using Orbit.Core.Contracts;

namespace Orbit.Engine;

/// <summary>
/// Main background service. Starts the hotkey listener and keeps the engine alive.
/// </summary>
public class Worker(
    IHotkeyService hotkeyService,
    ILogger<Worker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Orbit Engine started");

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
}
