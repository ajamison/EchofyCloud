using Echofy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Echofy.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.Phone).HasMaxLength(50);

        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(x => x.Street).HasMaxLength(300);
            a.Property(x => x.City).HasMaxLength(100);
            a.Property(x => x.Province).HasMaxLength(100);
            a.Property(x => x.Country).HasMaxLength(100);
            a.Property(x => x.PostalCode).HasMaxLength(20);
        });

        builder.HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Orders).WithOne(o => o.Customer).HasForeignKey(o => o.CustomerId);
    }
}
