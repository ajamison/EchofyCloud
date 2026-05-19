using Echofy.Domain.Entities;
using Echofy.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class FavoriteProductConfiguration : IEntityTypeConfiguration<FavoriteProduct>
{
    public void Configure(EntityTypeBuilder<FavoriteProduct> builder)
    {
        builder.HasKey(f => new { f.AppUserId, f.ProductId });

        builder.Property(f => f.AppUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(f => f.Product)
            .WithMany(p => p.FavoritedBy)
            .HasForeignKey(f => f.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(f => f.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
