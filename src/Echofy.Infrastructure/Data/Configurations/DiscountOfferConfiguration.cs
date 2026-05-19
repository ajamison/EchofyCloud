using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class DiscountOfferConfiguration : IEntityTypeConfiguration<DiscountOffer>
{
    public void Configure(EntityTypeBuilder<DiscountOffer> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(500);
        builder.Property(d => d.DiscountValue).HasPrecision(18, 2);

        builder.HasOne(d => d.Product)
            .WithMany(p => p.DiscountOffers)
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.ProductId, d.StartDate, d.EndDate });
    }
}
