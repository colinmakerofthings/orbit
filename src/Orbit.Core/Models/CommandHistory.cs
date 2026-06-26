namespace Orbit.Core.Models;

public class CommandHistory
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Command { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public required string Result { get; init; }
}
