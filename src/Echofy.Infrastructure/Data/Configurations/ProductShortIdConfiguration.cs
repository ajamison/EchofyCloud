using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class ProductShortIdConfiguration : IEntityTypeConfiguration<ProductShortId>
{
    public void Configure(EntityTypeBuilder<ProductShortId> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).IsRequired().HasMaxLength(12);
        builder.Property(s => s.Label).HasMaxLength(100);
        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasIndex(s => s.ProductId);

        builder.HasOne(s => s.Product)
            .WithMany(p => p.AdditionalShortIds)
            .HasForeignKey(s => s.ProductId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);  // unassign label when product is deleted
    }
}
