namespace Echofy.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public List<int> CategoryIds { get; set; } = [];
    public List<string> CategoryNames { get; set; } = [];
    public string? Sku { get; set; }
    public string? ManufacturerUpc { get; set; }
    public string? ImageUrl { get; set; }
    public string? Size { get; set; }
    public int? ManufacturerId { get; set; }
    public string? ManufacturerName { get; set; }
    public string? ManufacturerWebsite { get; set; }
    public int? ManufacturerProductId { get; set; }
    public string? ManufacturerPartNumber { get; set; }
    public int? UnitOfMeasureId { get; set; }
    public string? UnitOfMeasureName { get; set; }
    public string? UnitOfMeasureAbbreviation { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ShortId { get; set; }

    // Detail fields (populated by GetWithDetailsAsync)
    public IReadOnlyList<ProductImageDto> Images { get; set; } = [];
    public IReadOnlyList<ProductPriceHistoryDto> PriceHistory { get; set; } = [];
    public IReadOnlyList<DiscountOfferDto> DiscountOffers { get; set; } = [];
    public IReadOnlyList<ProductShortIdDto> AdditionalShortIds { get; set; } = [];

    public DiscountOfferDto? ActiveOffer =>
        DiscountOffers.FirstOrDefault(d => d.IsCurrentlyRunning);

    public decimal EffectivePrice
    {
        get
        {
            var offer = ActiveOffer;
            if (offer is null) return Price;
            return offer.DiscountType == Domain.Enums.DiscountType.Percentage
                ? Price * (1 - offer.DiscountValue / 100)
                : Math.Max(0, Price - offer.DiscountValue);
        }
    }
}
