namespace Orbit.Core.Models;

public class ActionResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? Error { get; init; }

    public static ActionResult Ok(string? message = null) =>
        new() { Success = true, Message = message };

    public static ActionResult Fail(string error) =>
        new() { Success = false, Error = error };
}
