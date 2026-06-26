using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbit.Persistence.Entities;

namespace Orbit.Persistence.Configurations;

public class WorkflowStepRunConfiguration : IEntityTypeConfiguration<WorkflowStepRunEntity>
{
    public void Configure(EntityTypeBuilder<WorkflowStepRunEntity> builder)
    {
        builder.ToTable("WorkflowStepRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StepName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Status).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Message).HasMaxLength(2000);
        builder.Property(x => x.Error).HasMaxLength(4000);
        builder.HasIndex(x => x.WorkflowRunId);
    }
}
