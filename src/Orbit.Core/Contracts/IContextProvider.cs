using Orbit.Core.Models;

namespace Orbit.Core.Contracts;

public interface IContextProvider
{
    Task<ContextData> GetCurrentContextAsync();
}
