using Echofy.Domain.Entities;
using Echofy.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.AppUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.Rating)
            .IsRequired();

        // One review per user per product
        builder.HasIndex(r => new { r.AppUserId, r.ProductId })
            .IsUnique();

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to AspNetUsers — restrict so deleting a user doesn't cascade to reviews
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
