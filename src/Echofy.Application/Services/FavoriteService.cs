using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class FavoriteService(IFavoriteProductRepository repo) : IFavoriteService
{
    public async Task<IReadOnlyList<ProductDto>> GetFavoritesAsync(string userId, CancellationToken ct = default)
    {
        var favorites = await repo.GetFavoritesForUserAsync(userId, ct);
        return favorites.Select(f => MapProductSlim(f.Product)).ToList();
    }

    public async Task<bool> IsFavoritedAsync(string userId, int productId, CancellationToken ct = default)
        => await repo.FindAsync(userId, productId, ct) is not null;

    public async Task<bool> ToggleFavoriteAsync(string userId, int productId, CancellationToken ct = default)
    {
        var existing = await repo.FindAsync(userId, productId, ct);
        if (existing is not null)
        {
            repo.Remove(existing);
            await repo.SaveChangesAsync(ct);
            return false;
        }

        await repo.AddAsync(new FavoriteProduct
        {
            AppUserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow
        }, ct);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    private static ProductDto MapProductSlim(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        ImageUrl = p.ImageUrl,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        CategoryIds = p.Categories.Select(c => c.Id).ToList(),
        CategoryNames = p.Categories.Select(c => c.Name).ToList(),
        CompanyId = p.CompanyId,
        AdditionalShortIds = p.AdditionalShortIds
            .Select(s => new ProductShortIdDto { Id = s.Id, Code = s.Code, ProductId = s.ProductId, AssignedAt = s.AssignedAt, CreatedAt = s.CreatedAt })
            .ToList()
    };
}
