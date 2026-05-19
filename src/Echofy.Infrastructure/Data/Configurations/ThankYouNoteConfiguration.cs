using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ThankYouNoteConfiguration : IEntityTypeConfiguration<ThankYouNote>
{
    public void Configure(EntityTypeBuilder<ThankYouNote> builder)
    {
        builder.HasKey(n => n.Id);
        builder.HasIndex(n => n.InvoiceId).IsUnique();

        builder.Property(n => n.CustomerEmail).IsRequired().HasMaxLength(256);
        builder.Property(n => n.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(n => n.CustomMessage).HasMaxLength(2000);
        builder.Property(n => n.ReferralCode).HasMaxLength(20);
        builder.Property(n => n.ReferralUrl).HasMaxLength(500);

        builder.HasOne(n => n.Invoice)
            .WithOne(i => i.ThankYouNote)
            .HasForeignKey<ThankYouNote>(n => n.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
