using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ManufacturerProductImageConfiguration : IEntityTypeConfiguration<ManufacturerProductImage>
{
    public void Configure(EntityTypeBuilder<ManufacturerProductImage> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.FileName).IsRequired().HasMaxLength(260);
        builder.Property(i => i.AltText).HasMaxLength(300);

        builder.HasOne(i => i.ManufacturerProduct)
            .WithMany(mp => mp.Images)
            .HasForeignKey(i => i.ManufacturerProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.ManufacturerProductId, i.DisplayOrder });
    }
}
