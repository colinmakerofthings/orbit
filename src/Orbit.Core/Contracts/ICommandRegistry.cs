using Orbit.Core.Models;

namespace Orbit.Core.Contracts;

public interface ICommandRegistry
{
    void Register(OrbitCommand command);
    OrbitCommand? Find(string name);
    IReadOnlyList<OrbitCommand> GetAll();
}
