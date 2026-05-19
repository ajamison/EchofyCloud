using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Title).IsRequired().HasMaxLength(300);
        builder.Property(d => d.Value).HasPrecision(18, 2);
        builder.Property(d => d.Stage).HasConversion<string>();
    }
}
