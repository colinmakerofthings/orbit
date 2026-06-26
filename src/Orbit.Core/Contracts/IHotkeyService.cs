namespace Orbit.Core.Contracts;

public interface IHotkeyService
{
    void Register(string hotkey, Func<Task> handler);
    void Unregister(string hotkey);
    void Start();
    void Stop();
}
