using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbit.Persistence.Entities;

namespace Orbit.Persistence.Configurations;

public class WorkflowRunConfiguration : IEntityTypeConfiguration<WorkflowRunEntity>
{
    public void Configure(EntityTypeBuilder<WorkflowRunEntity> builder)
    {
        builder.ToTable("WorkflowRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.WorkflowName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.WorkflowName);
        builder.HasIndex(x => x.StartedAt);
        builder.HasMany(x => x.StepRuns)
               .WithOne(x => x.WorkflowRun)
               .HasForeignKey(x => x.WorkflowRunId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
