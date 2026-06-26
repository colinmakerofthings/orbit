using Orbit.Core.Contracts;
using Orbit.Core.Models;

namespace Orbit.Engine.Engine;

public class CommandRegistry : ICommandRegistry
{
    private readonly Dictionary<string, OrbitCommand> _commands =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(OrbitCommand command) =>
        _commands[command.Name] = command;

    public OrbitCommand? Find(string name) =>
        _commands.TryGetValue(name, out var cmd) ? cmd : null;

    public IReadOnlyList<OrbitCommand> GetAll() =>
        _commands.Values.ToList();
}
