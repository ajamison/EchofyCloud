using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();

        builder.Property(i => i.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.CustomerEmail).IsRequired().HasMaxLength(256);
        builder.Property(i => i.CustomerPhone).HasMaxLength(50);
        builder.Property(i => i.AppUserId).HasMaxLength(450);
        builder.Property(i => i.Notes).HasMaxLength(2000);

        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property(i => i.TaxRate).HasColumnType("decimal(5,4)");
        builder.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.RewardGiftCardAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.RewardGiftCardCode).HasMaxLength(50);
        builder.Property(i => i.CreatedByUserId).HasMaxLength(450);

        builder.HasMany(i => i.Items)
            .WithOne(item => item.Invoice)
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Company relationship is configured in CompanyConfiguration

        builder.Ignore(i => i.SubTotal);
        builder.Ignore(i => i.TaxAmount);
        builder.Ignore(i => i.Total);
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Description).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Quantity).HasColumnType("decimal(10,3)");
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Ignore(i => i.Amount);
    }
}
