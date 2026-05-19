using Echofy.RecommendationApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Echofy.RecommendationApi.Data;

public class RecommendationDbContext(DbContextOptions<RecommendationDbContext> options)
    : DbContext(options)
{
    public DbSet<RProduct> Products => Set<RProduct>();
    public DbSet<RCategory> Categories => Set<RCategory>();
    public DbSet<RReview> Reviews => Set<RReview>();
    public DbSet<RFavoriteProduct> FavoriteProducts => Set<RFavoriteProduct>();
    public DbSet<RProductImage> ProductImages => Set<RProductImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Map to existing table names
        modelBuilder.Entity<RProduct>(b =>
        {
            b.ToTable("Products");
            b.HasKey(p => p.Id);
            b.Property(p => p.ShortId).HasMaxLength(12);
            b.Property(p => p.Name).HasMaxLength(300);
            b.Property(p => p.Price).HasPrecision(18, 2);

            // Many-to-many with Category using existing join table
            b.HasMany(p => p.Categories)
             .WithMany(c => c.Products)
             .UsingEntity("ProductCategories",
                 l => l.HasOne(typeof(RCategory)).WithMany().HasForeignKey("CategoriesId"),
                 r => r.HasOne(typeof(RProduct)).WithMany().HasForeignKey("ProductsId"));

            b.HasMany(p => p.Images)
             .WithOne()
             .HasForeignKey(i => i.ProductId);
        });

        modelBuilder.Entity<RCategory>(b =>
        {
            b.ToTable("Categories");
            b.HasKey(c => c.Id);
            b.Property(c => c.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<RReview>(b =>
        {
            b.ToTable("Reviews");
            b.HasKey(r => r.Id);
            b.Property(r => r.AppUserId).HasMaxLength(450);
        });

        modelBuilder.Entity<RFavoriteProduct>(b =>
        {
            b.ToTable("FavoriteProducts");
            b.HasKey(f => new { f.AppUserId, f.ProductId });
            b.Property(f => f.AppUserId).HasMaxLength(450);
        });

        modelBuilder.Entity<RProductImage>(b =>
        {
            b.ToTable("ProductImages");
            b.HasKey(i => i.Id);
            b.Property(i => i.FileName).HasMaxLength(260);
        });
    }
}
