using Echofy.Domain.Entities;
using Echofy.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Total).HasPrecision(18, 2);
        builder.Property(o => o.PaymentStatus).HasConversion<string>();
        builder.Property(o => o.FulfillmentStatus).HasConversion<string>();
        builder.Property(o => o.DeliveryType).HasConversion<string>();

        builder.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId);
    }
}
