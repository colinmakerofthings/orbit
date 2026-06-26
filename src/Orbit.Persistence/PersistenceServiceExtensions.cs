using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orbit.Core.Contracts;
using Orbit.Persistence.Repositories;

namespace Orbit.Persistence;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddOrbitPersistence(
        this IServiceCollection services,
        string databasePath)
    {
        services.AddDbContext<OrbitDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddScoped<IWorkflowRepository, WorkflowRepository>();

        return services;
    }
}
