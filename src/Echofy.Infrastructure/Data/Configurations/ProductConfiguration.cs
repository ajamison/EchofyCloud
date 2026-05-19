using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.ShortId).IsRequired(false).HasMaxLength(12);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.Property(p => p.Sku).HasMaxLength(100);
        builder.HasIndex(p => p.Sku).IsUnique().HasFilter("\"Sku\" IS NOT NULL");
        builder.Property(p => p.ManufacturerUpc).HasMaxLength(50);
        builder.Property(p => p.Size).HasMaxLength(50);

        builder.HasOne(p => p.Manufacturer)
            .WithMany(m => m.Products)
            .HasForeignKey(p => p.ManufacturerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.UnitOfMeasure)
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.ManufacturerProduct)
            .WithMany(mp => mp.Products)
            .HasForeignKey(p => p.ManufacturerProductId)
            .OnDelete(DeleteBehavior.SetNull);

        // Company relationship is configured in CompanyConfiguration

        builder.HasMany(p => p.OrderItems).WithOne(i => i.Product).HasForeignKey(i => i.ProductId);
        builder.HasMany(p => p.Reviews).WithOne(r => r.Product).HasForeignKey(r => r.ProductId);

        // Many-to-many with Category
        builder.HasMany(p => p.Categories)
            .WithMany(c => c.Products)
            .UsingEntity("ProductCategories");
    }
}
