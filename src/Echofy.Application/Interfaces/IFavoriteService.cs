using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IFavoriteService
{
    Task<IReadOnlyList<ProductDto>> GetFavoritesAsync(string userId, CancellationToken ct = default);
    Task<bool> IsFavoritedAsync(string userId, int productId, CancellationToken ct = default);
    /// <summary>Returns true if the product is now favorited, false if it was removed.</summary>
    Task<bool> ToggleFavoriteAsync(string userId, int productId, CancellationToken ct = default);
}
