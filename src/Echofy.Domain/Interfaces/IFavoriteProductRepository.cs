using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IFavoriteProductRepository
{
    Task<IReadOnlyList<FavoriteProduct>> GetFavoritesForUserAsync(string userId, CancellationToken ct = default);
    Task<FavoriteProduct?> FindAsync(string userId, int productId, CancellationToken ct = default);
    Task AddAsync(FavoriteProduct favorite, CancellationToken ct = default);
    void Remove(FavoriteProduct favorite);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
