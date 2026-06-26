namespace Orbit.Persistence.Entities;

public class CommandHistoryEntity
{
    public Guid Id { get; set; }
    public string Command { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Result { get; set; } = string.Empty;
}
