using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Website).HasMaxLength(300);
        builder.Property(c => c.TaxNumber).HasMaxLength(50);
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Country).HasMaxLength(100);

        builder.HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Products)
            .WithOne(p => p.Company)
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Invoices)
            .WithOne(i => i.Company)
            .HasForeignKey(i => i.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
