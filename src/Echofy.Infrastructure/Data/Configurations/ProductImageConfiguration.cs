using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.FileName).IsRequired().HasMaxLength(260);
        builder.Property(i => i.AltText).HasMaxLength(300);
        builder.Property(i => i.Sku).HasMaxLength(100);
        builder.HasIndex(i => i.Sku).HasFilter("\"Sku\" IS NOT NULL");

        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.ProductId, i.DisplayOrder });
    }
}
