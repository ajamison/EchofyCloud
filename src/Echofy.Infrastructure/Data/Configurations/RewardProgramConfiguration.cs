using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class RewardProgramConfiguration : IEntityTypeConfiguration<RewardProgram>
{
    public void Configure(EntityTypeBuilder<RewardProgram> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);

        builder.HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Tiers)
            .WithOne(t => t.RewardProgram)
            .HasForeignKey(t => t.RewardProgramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RewardTierConfiguration : IEntityTypeConfiguration<RewardTier>
{
    public void Configure(EntityTypeBuilder<RewardTier> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Label).IsRequired().HasMaxLength(100);
        builder.Property(t => t.MinInvoiceAmount).HasColumnType("decimal(18,2)");
        builder.Property(t => t.GiftCardAmount).HasColumnType("decimal(18,2)");
    }
}
