using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class FavoriteProductRepository(ApplicationDbContext db) : IFavoriteProductRepository
{
    public async Task<IReadOnlyList<FavoriteProduct>> GetFavoritesForUserAsync(string userId, CancellationToken ct = default)
        => await db.FavoriteProducts
            .Where(f => f.AppUserId == userId)
            .Include(f => f.Product)
                .ThenInclude(p => p.Categories)
            .Include(f => f.Product)
                .ThenInclude(p => p.AdditionalShortIds.OrderBy(s => s.CreatedAt))
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);

    public async Task<FavoriteProduct?> FindAsync(string userId, int productId, CancellationToken ct = default)
        => await db.FavoriteProducts
            .FirstOrDefaultAsync(f => f.AppUserId == userId && f.ProductId == productId, ct);

    public async Task AddAsync(FavoriteProduct favorite, CancellationToken ct = default)
        => await db.FavoriteProducts.AddAsync(favorite, ct);

    public void Remove(FavoriteProduct favorite)
        => db.FavoriteProducts.Remove(favorite);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
