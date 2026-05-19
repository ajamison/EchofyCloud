using Echofy.Domain.Entities;
using Echofy.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ReferralRewardConfiguration : IEntityTypeConfiguration<ReferralReward>
{
    public void Configure(EntityTypeBuilder<ReferralReward> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.AppUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(r => r.PointsEarned)
            .HasDefaultValue(0);

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.HasOne(r => r.ReferralUse)
            .WithOne(u => u.Reward)
            .HasForeignKey<ReferralReward>(r => r.ReferralUseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(r => r.AppUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
