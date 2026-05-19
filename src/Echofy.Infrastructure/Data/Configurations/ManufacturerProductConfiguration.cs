using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ManufacturerProductConfiguration : IEntityTypeConfiguration<ManufacturerProduct>
{
    public void Configure(EntityTypeBuilder<ManufacturerProduct> builder)
    {
        builder.HasKey(mp => mp.Id);
        builder.Property(mp => mp.Name).IsRequired().HasMaxLength(300);
        builder.Property(mp => mp.ManufacturerPartNumber).HasMaxLength(100);
        builder.Property(mp => mp.Sku).HasMaxLength(100);
        builder.Property(mp => mp.Size).HasMaxLength(100);
        builder.Property(mp => mp.Msrp).HasColumnType("decimal(18,2)");

        builder.HasOne(mp => mp.Manufacturer)
            .WithMany(m => m.ManufacturerProducts)
            .HasForeignKey(mp => mp.ManufacturerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(mp => mp.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(mp => mp.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
