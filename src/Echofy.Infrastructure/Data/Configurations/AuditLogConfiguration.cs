using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(20);
        builder.Property(a => a.EntityId).HasMaxLength(50);
        builder.Property(a => a.ChangedByUserId).HasMaxLength(450);

        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.ChangedAt);
    }
}
