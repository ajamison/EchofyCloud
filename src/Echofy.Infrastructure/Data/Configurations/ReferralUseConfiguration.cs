using Echofy.Domain.Entities;
using Echofy.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ReferralUseConfiguration : IEntityTypeConfiguration<ReferralUse>
{
    public void Configure(EntityTypeBuilder<ReferralUse> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UsedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(u => u.UsedByEmail)
            .IsRequired()
            .HasMaxLength(256);

        // One user can only use one referral code
        builder.HasIndex(u => u.UsedByUserId).IsUnique();

        builder.HasOne(u => u.ReferralCode)
            .WithMany(c => c.Uses)
            .HasForeignKey(u => u.ReferralCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(u => u.UsedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
