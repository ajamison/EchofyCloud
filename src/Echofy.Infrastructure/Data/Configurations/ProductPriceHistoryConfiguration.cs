using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ProductPriceHistoryConfiguration : IEntityTypeConfiguration<ProductPriceHistory>
{
    public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Price).HasPrecision(18, 2);
        builder.Property(h => h.ChangedByUserId).HasMaxLength(450);

        builder.HasOne(h => h.Product)
            .WithMany(p => p.PriceHistory)
            .HasForeignKey(h => h.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => new { h.ProductId, h.EffectiveTo });
    }
}
