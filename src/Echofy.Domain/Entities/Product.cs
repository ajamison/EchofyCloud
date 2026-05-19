using Echofy.Domain.Interfaces;

namespace Echofy.Domain.Entities;

public class Product : IAuditable
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string? ShortId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }                  // legacy single image, kept for compatibility
    public string? Sku { get; set; }
    public string? ManufacturerUpc { get; set; }
    public string? Size { get; set; }
    public int? ManufacturerId { get; set; }
    public int? ManufacturerProductId { get; set; }
    public int? UnitOfMeasureId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }

    public Company? Company { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public ManufacturerProduct? ManufacturerProduct { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    public ICollection<Category> Categories { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<ProductPriceHistory> PriceHistory { get; set; } = [];
    public ICollection<DiscountOffer> DiscountOffers { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<FavoriteProduct> FavoritedBy { get; set; } = [];
    public ICollection<ProductShortId> AdditionalShortIds { get; set; } = [];
}
