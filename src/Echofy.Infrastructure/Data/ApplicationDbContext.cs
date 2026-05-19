using System.Text.Json;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Echofy.Infrastructure.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentUserService currentUser)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductPriceHistory> ProductPriceHistories => Set<ProductPriceHistory>();
    public DbSet<DiscountOffer> DiscountOffers => Set<DiscountOffer>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<FavoriteProduct> FavoriteProducts => Set<FavoriteProduct>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<ProductShortId> ProductShortIds => Set<ProductShortId>();
    public DbSet<ManufacturerProduct> ManufacturerProducts => Set<ManufacturerProduct>();
    public DbSet<ManufacturerProductImage> ManufacturerProductImages => Set<ManufacturerProductImage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ReferralCode> ReferralCodes => Set<ReferralCode>();
    public DbSet<ReferralUse> ReferralUses => Set<ReferralUse>();
    public DbSet<ReferralReward> ReferralRewards => Set<ReferralReward>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<ThankYouNote> ThankYouNotes => Set<ThankYouNote>();
    public DbSet<RewardProgram> RewardPrograms => Set<RewardProgram>();
    public DbSet<RewardTier> RewardTiers => Set<RewardTier>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = currentUser.UserId;
        var auditEntries = new List<(AuditLog Entry, EntityEntry EntityEntry)>();

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedByUserId = userId;
            }

            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                var action = entry.State switch
                {
                    EntityState.Added    => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted  => "Deleted",
                    _                    => ""
                };

                var oldValues = entry.State is EntityState.Modified or EntityState.Deleted
                    ? JsonSerializer.Serialize(entry.OriginalValues.ToObject())
                    : null;

                var newValues = entry.State is EntityState.Added or EntityState.Modified
                    ? JsonSerializer.Serialize(entry.CurrentValues.ToObject())
                    : null;

                var log = new AuditLog
                {
                    EntityName      = entry.Entity.GetType().Name,
                    Action          = action,
                    OldValues       = oldValues,
                    NewValues       = newValues,
                    ChangedByUserId = userId,
                    ChangedAt       = now
                };

                auditEntries.Add((log, entry));
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Populate EntityId after save so auto-increment PKs are resolved
        foreach (var (log, entry) in auditEntries)
        {
            log.EntityId = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey())
                ?.CurrentValue?.ToString();
            AuditLogs.Add(log);
        }

        if (auditEntries.Count > 0)
            await base.SaveChangesAsync(cancellationToken);

        return result;
    }
}
