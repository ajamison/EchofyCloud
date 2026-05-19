using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class ProductRepository(ApplicationDbContext db)
    : GenericRepository<Product>(db), IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetFilteredAsync(int? clientId, string? search, bool? activeOnly = null, CancellationToken ct = default)
    {
        var q = Db.Products
            .Include(p => p.Company)
            .Include(p => p.Categories)
            .Include(p => p.Manufacturer)
            .Include(p => p.ManufacturerProduct)
            .Include(p => p.UnitOfMeasure)
            .AsQueryable();

        if (clientId.HasValue)
            q = q.Where(p => p.Company != null && p.Company.ClientId == clientId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Name.Contains(search) || p.Categories.Any(c => c.Name.Contains(search)));

        if (activeOnly.HasValue)
            q = q.Where(p => p.IsActive == activeOnly.Value);

        return await q.OrderBy(p => p.Name).ToListAsync(ct);
    }

    public override async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Company)
            .Include(p => p.Categories)
            .Include(p => p.Manufacturer)
            .Include(p => p.ManufacturerProduct)
            .Include(p => p.UnitOfMeasure)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Company)
            .Include(p => p.Categories)
            .Include(p => p.Manufacturer)
            .Include(p => p.ManufacturerProduct)
            .Include(p => p.UnitOfMeasure)
            .Where(p => p.Name.Contains(term) || p.Categories.Any(c => c.Name.Contains(term)))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(int threshold = 5, CancellationToken ct = default) =>
        await Db.Products
            .Where(p => p.StockQuantity <= threshold && p.IsActive)
            .ToListAsync(ct);

    public async Task<int> CountOutOfStockAsync(CancellationToken ct = default) =>
        await Db.Products.CountAsync(p => p.StockQuantity == 0 && p.IsActive, ct);

    public async Task<Product?> GetWithDetailsAsync(int id, CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Company)
            .Include(p => p.Manufacturer)
            .Include(p => p.ManufacturerProduct)
            .Include(p => p.UnitOfMeasure)
            .Include(p => p.Categories)
            .Include(p => p.PriceHistory.OrderByDescending(h => h.EffectiveFrom))
            .Include(p => p.DiscountOffers.OrderByDescending(d => d.StartDate))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.AdditionalShortIds.OrderBy(s => s.CreatedAt))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Product?> GetByShortIdAsync(string shortId, CancellationToken ct = default)
    {
        var productId = await Db.ProductShortIds
            .Where(s => s.Code == shortId && s.ProductId != null)
            .Select(s => s.ProductId)
            .FirstOrDefaultAsync(ct);

        if (productId is null) return null;

        return await Db.Products
            .Include(p => p.Company)
            .Include(p => p.Manufacturer)
            .Include(p => p.UnitOfMeasure)
            .Include(p => p.Categories)
            .Include(p => p.DiscountOffers.OrderByDescending(d => d.StartDate))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == productId && p.IsActive, ct);
    }

    public async Task<Product?> GetByUpcAsync(string upc, CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Company)
            .Include(p => p.Manufacturer)
            .Include(p => p.UnitOfMeasure)
            .Include(p => p.Categories)
            .Include(p => p.DiscountOffers.OrderByDescending(d => d.StartDate))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.ManufacturerUpc == upc && p.IsActive, ct);

    public async Task<Product?> GetProductWithCategoriesAsync(int id, CancellationToken ct = default) =>
        await Db.Products
            .Include(p => p.Company)
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    // ── Price history ──────────────────────────────────────────────────────

    public async Task<ProductPriceHistory?> GetCurrentPriceHistoryAsync(int productId, CancellationToken ct = default) =>
        await Db.ProductPriceHistories
            .FirstOrDefaultAsync(h => h.ProductId == productId && h.EffectiveTo == null, ct);

    public async Task<IReadOnlyList<ProductPriceHistory>> GetPriceHistoryAsync(int productId, CancellationToken ct = default) =>
        await Db.ProductPriceHistories
            .Where(h => h.ProductId == productId)
            .OrderByDescending(h => h.EffectiveFrom)
            .ToListAsync(ct);

    public async Task AddPriceHistoryAsync(ProductPriceHistory entry, CancellationToken ct = default) =>
        await Db.ProductPriceHistories.AddAsync(entry, ct);

    // ── Discount offers ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DiscountOffer>> GetDiscountOffersAsync(int productId, CancellationToken ct = default) =>
        await Db.DiscountOffers
            .Where(d => d.ProductId == productId)
            .OrderByDescending(d => d.StartDate)
            .ToListAsync(ct);

    public async Task<DiscountOffer?> GetDiscountOfferByIdAsync(int productId, int offerId, CancellationToken ct = default) =>
        await Db.DiscountOffers
            .FirstOrDefaultAsync(d => d.Id == offerId && d.ProductId == productId, ct);

    public async Task AddDiscountOfferAsync(DiscountOffer offer, CancellationToken ct = default) =>
        await Db.DiscountOffers.AddAsync(offer, ct);

    public void UpdateDiscountOffer(DiscountOffer offer) =>
        Db.DiscountOffers.Update(offer);

    public void DeleteDiscountOffer(DiscountOffer offer) =>
        Db.DiscountOffers.Remove(offer);

    // ── Images ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ProductImage>> GetImagesAsync(int productId, CancellationToken ct = default) =>
        await Db.ProductImages
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync(ct);

    public async Task<ProductImage?> GetImageByIdAsync(int productId, int imageId, CancellationToken ct = default) =>
        await Db.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId, ct);

    public async Task AddImageAsync(ProductImage image, CancellationToken ct = default) =>
        await Db.ProductImages.AddAsync(image, ct);

    public void UpdateImage(ProductImage image) =>
        Db.ProductImages.Update(image);

    public void DeleteImage(ProductImage image) =>
        Db.ProductImages.Remove(image);

    // ── Additional ShortIds ────────────────────────────────────────────────

    public async Task<IReadOnlyList<ProductShortId>> GetShortIdsAsync(int productId, CancellationToken ct = default) =>
        await Db.ProductShortIds
            .Where(s => s.ProductId == productId)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProductShortId>> GetAllShortIdsAsync(bool? assigned = null, CancellationToken ct = default)
    {
        var query = Db.ProductShortIds.Include(s => s.Product).AsQueryable();
        if (assigned == true)  query = query.Where(s => s.ProductId != null);
        if (assigned == false) query = query.Where(s => s.ProductId == null);
        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
    }

    public async Task<ProductShortId?> GetShortIdByIdAsync(int productId, int shortIdId, CancellationToken ct = default) =>
        await Db.ProductShortIds
            .FirstOrDefaultAsync(s => s.Id == shortIdId && s.ProductId == productId, ct);

    public async Task<ProductShortId?> GetShortIdByIdAsync(int shortIdId, CancellationToken ct = default) =>
        await Db.ProductShortIds
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.Id == shortIdId, ct);

    public async Task<bool> ShortIdCodeExistsAsync(string code, CancellationToken ct = default) =>
        await Db.ProductShortIds.AnyAsync(s => s.Code == code, ct);

    public async Task AddShortIdAsync(ProductShortId sid, CancellationToken ct = default) =>
        await Db.ProductShortIds.AddAsync(sid, ct);

    public void UpdateShortId(ProductShortId sid) =>
        Db.ProductShortIds.Update(sid);

    public void DeleteShortId(ProductShortId sid) =>
        Db.ProductShortIds.Remove(sid);
}
