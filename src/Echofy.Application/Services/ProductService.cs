using System.Security.Cryptography;
using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class ProductService(IProductRepository repo, ICategoryRepository categoryRepo) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(int? clientId = null, string? search = null, bool? activeOnly = null, CancellationToken ct = default)
    {
        var products = await repo.GetFilteredAsync(clientId, search, activeOnly, ct);
        return products.Select(MapSlim).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var p = await repo.GetByIdAsync(id, ct);
        return p is null ? null : MapSlim(p);
    }

    public async Task<ProductDto?> GetByShortIdAsync(string shortId, CancellationToken ct = default)
    {
        var p = await repo.GetByShortIdAsync(shortId, ct);
        return p is null ? null : MapDetail(p);
    }

    public async Task<ProductDto?> GetByUpcAsync(string upc, CancellationToken ct = default)
    {
        var p = await repo.GetByUpcAsync(upc, ct);
        return p is null ? null : MapDetail(p);
    }

    public async Task<ProductDto?> GetWithDetailsAsync(int id, CancellationToken ct = default)
    {
        var p = await repo.GetWithDetailsAsync(id, ct);
        return p is null ? null : MapDetail(p);
    }

    public async Task<ProductDto> CreateAsync(ProductDto dto, string? changedByUserId = null, CancellationToken ct = default)
    {
        var product = new Product
        {
            CompanyId = dto.CompanyId,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Sku = dto.Sku,
            ManufacturerUpc = dto.ManufacturerUpc,
            ImageUrl = dto.ImageUrl,
            Size = dto.Size,
            ManufacturerId = dto.ManufacturerId,
            ManufacturerProductId = dto.ManufacturerProductId,
            UnitOfMeasureId = dto.UnitOfMeasureId,
            IsActive = dto.IsActive
        };

        if (dto.CategoryIds.Count > 0)
        {
            var cats = await categoryRepo.GetByIdsAsync(dto.CategoryIds, ct);
            foreach (var cat in cats) product.Categories.Add(cat);
        }

        await repo.AddAsync(product, ct);
        await repo.SaveChangesAsync(ct);

        // Record the initial price in history
        await repo.AddPriceHistoryAsync(new ProductPriceHistory
        {
            ProductId = product.Id,
            Price = product.Price,
            EffectiveFrom = product.CreatedAt,
            EffectiveTo = null,
            ChangedByUserId = changedByUserId
        }, ct);
        await repo.SaveChangesAsync(ct);

        dto.Id = product.Id;
        return dto;
    }

    public async Task<ProductDto?> UpdateAsync(int id, ProductDto dto, int? clientId = null, string? changedByUserId = null, CancellationToken ct = default)
    {
        var product = await repo.GetProductWithCategoriesAsync(id, ct);
        if (product is null) return null;
        if (clientId.HasValue && product.Company?.ClientId != clientId.Value) return null;

        var priceChanged = product.Price != dto.Price;

        product.CompanyId = dto.CompanyId;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.Sku = dto.Sku;
        product.ManufacturerUpc = dto.ManufacturerUpc;
        product.ImageUrl = dto.ImageUrl;
        product.Size = dto.Size;
        product.ManufacturerId = dto.ManufacturerId;
        product.ManufacturerProductId = dto.ManufacturerProductId;
        product.UnitOfMeasureId = dto.UnitOfMeasureId;
        product.IsActive = dto.IsActive;

        // Update categories
        product.Categories.Clear();
        if (dto.CategoryIds.Count > 0)
        {
            var cats = await categoryRepo.GetByIdsAsync(dto.CategoryIds, ct);
            foreach (var cat in cats) product.Categories.Add(cat);
        }

        if (priceChanged)
        {
            var now = DateTime.UtcNow;

            // Close the current open price history entry
            var current = await repo.GetCurrentPriceHistoryAsync(id, ct);
            if (current is not null)
                current.EffectiveTo = now;

            // Open a new price history entry
            await repo.AddPriceHistoryAsync(new ProductPriceHistory
            {
                ProductId = id,
                Price = dto.Price,
                EffectiveFrom = now,
                EffectiveTo = null,
                ChangedByUserId = changedByUserId
            }, ct);
        }

        await repo.SaveChangesAsync(ct);
        return dto;
    }

    public async Task<bool> DeleteAsync(int id, int? clientId = null, CancellationToken ct = default)
    {
        var product = await repo.GetByIdAsync(id, ct);
        if (product is null) return false;
        if (clientId.HasValue && product.Company?.ClientId != clientId.Value) return false;

        repo.Delete(product);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    // ── Discount offers ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<DiscountOfferDto>> GetDiscountOffersAsync(int productId, CancellationToken ct = default)
    {
        var offers = await repo.GetDiscountOffersAsync(productId, ct);
        return offers.Select(MapOffer).ToList();
    }

    public async Task<DiscountOfferDto> CreateDiscountOfferAsync(int productId, DiscountOfferDto dto, CancellationToken ct = default)
    {
        var offer = new DiscountOffer
        {
            ProductId = productId,
            Name = dto.Name,
            Description = dto.Description,
            DiscountType = dto.DiscountType,
            DiscountValue = dto.DiscountValue,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            IsActive = dto.IsActive
        };

        await repo.AddDiscountOfferAsync(offer, ct);
        await repo.SaveChangesAsync(ct);
        dto.Id = offer.Id;
        dto.ProductId = productId;
        return dto;
    }

    public async Task<DiscountOfferDto?> UpdateDiscountOfferAsync(int productId, int offerId, DiscountOfferDto dto, CancellationToken ct = default)
    {
        var offer = await repo.GetDiscountOfferByIdAsync(productId, offerId, ct);
        if (offer is null) return null;

        offer.Name = dto.Name;
        offer.Description = dto.Description;
        offer.DiscountType = dto.DiscountType;
        offer.DiscountValue = dto.DiscountValue;
        offer.StartDate = dto.StartDate;
        offer.EndDate = dto.EndDate;
        offer.IsActive = dto.IsActive;

        repo.UpdateDiscountOffer(offer);
        await repo.SaveChangesAsync(ct);
        return MapOffer(offer);
    }

    public async Task<bool> DeleteDiscountOfferAsync(int productId, int offerId, CancellationToken ct = default)
    {
        var offer = await repo.GetDiscountOfferByIdAsync(productId, offerId, ct);
        if (offer is null) return false;

        repo.DeleteDiscountOffer(offer);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    // ── Images ─────────────────────────────────────────────────────────────

    public async Task<ProductImageDto> AddImageAsync(int productId, string fileName, string? altText, string? sku = null, CancellationToken ct = default)
    {
        var existing = await repo.GetImagesAsync(productId, ct);
        var isFirst = existing.Count == 0;

        var image = new ProductImage
        {
            ProductId = productId,
            FileName = fileName,
            AltText = altText,
            Sku = sku,
            IsMain = isFirst,
            DisplayOrder = existing.Count
        };

        await repo.AddImageAsync(image, ct);
        await repo.SaveChangesAsync(ct);
        return MapImage(image);
    }

    public async Task<bool> DeleteImageAsync(int productId, int imageId, CancellationToken ct = default)
    {
        var image = await repo.GetImageByIdAsync(productId, imageId, ct);
        if (image is null) return false;

        var wasMain = image.IsMain;
        repo.DeleteImage(image);
        await repo.SaveChangesAsync(ct);

        // If the deleted image was the main one, promote the first remaining image
        if (wasMain)
        {
            var remaining = await repo.GetImagesAsync(productId, ct);
            if (remaining.Count > 0)
            {
                var next = remaining[0];
                next.IsMain = true;
                repo.UpdateImage(next);
                await repo.SaveChangesAsync(ct);
            }
        }

        return true;
    }

    public async Task<bool> SetMainImageAsync(int productId, int imageId, CancellationToken ct = default)
    {
        var images = await repo.GetImagesAsync(productId, ct);
        if (!images.Any(i => i.Id == imageId)) return false;

        foreach (var img in images)
        {
            img.IsMain = img.Id == imageId;
            repo.UpdateImage(img);
        }

        await repo.SaveChangesAsync(ct);
        return true;
    }

    // ── Mapping ────────────────────────────────────────────────────────────

    private static ProductDto MapSlim(Product p) => new()
    {
        Id = p.Id,
        CompanyId = p.CompanyId,
        CompanyName = p.Company?.Name,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        Sku = p.Sku,
        ManufacturerUpc = p.ManufacturerUpc,
        CategoryIds = p.Categories.Select(c => c.Id).ToList(),
        CategoryNames = p.Categories.Select(c => c.Name).ToList(),
        ImageUrl = p.ImageUrl,
        Size = p.Size,
        ManufacturerId = p.ManufacturerId,
        ManufacturerName = p.Manufacturer?.Name,
        ManufacturerWebsite = p.Manufacturer?.Website,
        ManufacturerProductId = p.ManufacturerProductId,
        ManufacturerPartNumber = p.ManufacturerProduct?.ManufacturerPartNumber,
        UnitOfMeasureId = p.UnitOfMeasureId,
        UnitOfMeasureName = p.UnitOfMeasure?.Name,
        UnitOfMeasureAbbreviation = p.UnitOfMeasure?.Abbreviation,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt
    };

    private static ProductDto MapDetail(Product p) => new()
    {
        Id = p.Id,
        CompanyId = p.CompanyId,
        CompanyName = p.Company?.Name,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        Sku = p.Sku,
        ManufacturerUpc = p.ManufacturerUpc,
        CategoryIds = p.Categories.Select(c => c.Id).ToList(),
        CategoryNames = p.Categories.Select(c => c.Name).ToList(),
        ImageUrl = p.ImageUrl,
        Size = p.Size,
        ManufacturerId = p.ManufacturerId,
        ManufacturerName = p.Manufacturer?.Name,
        ManufacturerWebsite = p.Manufacturer?.Website,
        ManufacturerProductId = p.ManufacturerProductId,
        ManufacturerPartNumber = p.ManufacturerProduct?.ManufacturerPartNumber,
        UnitOfMeasureId = p.UnitOfMeasureId,
        UnitOfMeasureName = p.UnitOfMeasure?.Name,
        UnitOfMeasureAbbreviation = p.UnitOfMeasure?.Abbreviation,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        PriceHistory = p.PriceHistory.Select(h => new ProductPriceHistoryDto
        {
            Id = h.Id,
            Price = h.Price,
            EffectiveFrom = h.EffectiveFrom,
            EffectiveTo = h.EffectiveTo
        }).ToList(),
        DiscountOffers = p.DiscountOffers.Select(MapOffer).ToList(),
        Images = p.Images.Select(MapImage).ToList(),
        AdditionalShortIds = p.AdditionalShortIds.Select(MapShortId).ToList()
    };

    private static DiscountOfferDto MapOffer(DiscountOffer d) => new()
    {
        Id = d.Id,
        ProductId = d.ProductId,
        Name = d.Name,
        Description = d.Description,
        DiscountType = d.DiscountType,
        DiscountValue = d.DiscountValue,
        StartDate = d.StartDate,
        EndDate = d.EndDate,
        IsActive = d.IsActive
    };

    private static ProductImageDto MapImage(ProductImage i) => new()
    {
        Id = i.Id,
        ProductId = i.ProductId,
        FileName = i.FileName,
        AltText = i.AltText,
        Sku = i.Sku,
        IsMain = i.IsMain,
        DisplayOrder = i.DisplayOrder,
        UploadedAt = i.UploadedAt
    };

    // ── Additional ShortIds ────────────────────────────────────────────────

    public async Task<ProductShortIdDto> GenerateAdditionalShortIdAsync(int productId, string? label, CancellationToken ct = default)
    {
        var sid = new ProductShortId { ProductId = productId, Code = GenerateShortId(), Label = label };
        await repo.AddShortIdAsync(sid, ct);
        await repo.SaveChangesAsync(ct);
        return MapShortId(sid);
    }

    public async Task<bool> DeleteAdditionalShortIdAsync(int productId, int shortIdId, CancellationToken ct = default)
    {
        var sid = await repo.GetShortIdByIdAsync(productId, shortIdId, ct);
        if (sid is null) return false;
        repo.DeleteShortId(sid);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    // ── Label pool ─────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<ProductShortIdDto>> GetAllShortIdsAsync(bool? assigned = null, CancellationToken ct = default)
    {
        var list = await repo.GetAllShortIdsAsync(assigned, ct);
        return list.Select(MapShortId).ToList();
    }

    public async Task<IReadOnlyList<ProductShortIdDto>> GenerateBatchShortIdsAsync(int count, string? label, CancellationToken ct = default)
    {
        count = Math.Clamp(count, 1, 100);
        var results = new List<ProductShortId>();
        for (var i = 0; i < count; i++)
        {
            string code;
            do { code = GenerateShortId(); }
            while (await repo.ShortIdCodeExistsAsync(code, ct));

            var sid = new ProductShortId { Code = code, Label = label };
            await repo.AddShortIdAsync(sid, ct);
            results.Add(sid);
        }
        await repo.SaveChangesAsync(ct);
        return results.Select(MapShortId).ToList();
    }

    public async Task<ProductShortIdDto?> AssignProductAsync(int shortIdId, int productId, CancellationToken ct = default)
    {
        var sid = await repo.GetShortIdByIdAsync(shortIdId, ct);
        if (sid is null) return null;
        sid.ProductId = productId;
        sid.AssignedAt = DateTime.UtcNow;
        repo.UpdateShortId(sid);
        await repo.SaveChangesAsync(ct);
        return MapShortId(sid);
    }

    public async Task<bool> UnassignShortIdAsync(int shortIdId, CancellationToken ct = default)
    {
        var sid = await repo.GetShortIdByIdAsync(shortIdId, ct);
        if (sid is null) return false;
        sid.ProductId = null;
        sid.AssignedAt = null;
        repo.UpdateShortId(sid);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteShortIdAsync(int shortIdId, CancellationToken ct = default)
    {
        var sid = await repo.GetShortIdByIdAsync(shortIdId, ct);
        if (sid is null) return false;
        repo.DeleteShortId(sid);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private static ProductShortIdDto MapShortId(ProductShortId s) => new()
    {
        Id = s.Id,
        ProductId = s.ProductId,
        ProductName = s.Product?.Name,
        Code = s.Code,
        Label = s.Label,
        CreatedAt = s.CreatedAt,
        AssignedAt = s.AssignedAt
    };

    private static string GenerateShortId()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var bytes = RandomNumberGenerator.GetBytes(8);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}
