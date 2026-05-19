using Echofy.Domain.Entities;
using Echofy.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ReferralCodeConfiguration : IEntityTypeConfiguration<ReferralCode>
{
    public void Configure(EntityTypeBuilder<ReferralCode> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.AppUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(r => r.Code).IsUnique();
        builder.HasIndex(r => r.AppUserId).IsUnique();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
