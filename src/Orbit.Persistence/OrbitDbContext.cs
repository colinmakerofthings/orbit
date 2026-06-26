using Microsoft.EntityFrameworkCore;
using Orbit.Persistence.Configurations;
using Orbit.Persistence.Entities;

namespace Orbit.Persistence;

public class OrbitDbContext(DbContextOptions<OrbitDbContext> options) : DbContext(options)
{
    public DbSet<WorkflowRunEntity> WorkflowRuns => Set<WorkflowRunEntity>();
    public DbSet<WorkflowStepRunEntity> WorkflowStepRuns => Set<WorkflowStepRunEntity>();
    public DbSet<CommandHistoryEntity> CommandHistory => Set<CommandHistoryEntity>();
    public DbSet<SettingEntity> Settings => Set<SettingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WorkflowRunConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowStepRunConfiguration());
        modelBuilder.ApplyConfiguration(new CommandHistoryConfiguration());
        modelBuilder.ApplyConfiguration(new SettingConfiguration());
    }
}
