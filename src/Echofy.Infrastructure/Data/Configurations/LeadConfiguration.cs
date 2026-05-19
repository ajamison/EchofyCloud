using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.FullName).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Email).IsRequired().HasMaxLength(256);
        builder.Property(l => l.EstimatedValue).HasPrecision(18, 2);
        builder.Property(l => l.Status).HasConversion<string>();

        builder.HasMany(l => l.Deals).WithOne(d => d.Lead).HasForeignKey(d => d.LeadId);
    }
}
