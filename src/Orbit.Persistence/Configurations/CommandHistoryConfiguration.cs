using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orbit.Persistence.Entities;

namespace Orbit.Persistence.Configurations;

public class CommandHistoryConfiguration : IEntityTypeConfiguration<CommandHistoryEntity>
{
    public void Configure(EntityTypeBuilder<CommandHistoryEntity> builder)
    {
        builder.ToTable("CommandHistory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Command).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Result).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Timestamp);
    }
}
