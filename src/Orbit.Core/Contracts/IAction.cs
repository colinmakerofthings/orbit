using Orbit.Core.Models;

namespace Orbit.Core.Contracts;

public interface IAction
{
    string Name { get; }

    Task<ActionResult> Execute(
        ActionContext context,
        IDictionary<string, object> parameters,
        CancellationToken cancellationToken = default);
}
