using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IReadOnlyList<Product>> GetFilteredAsync(int? clientId, string? search, bool? activeOnly = null, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> SearchAsync(string term, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(int threshold = 5, CancellationToken ct = default);
    Task<int> CountOutOfStockAsync(CancellationToken ct = default);

    Task<Product?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<Product?> GetByShortIdAsync(string shortId, CancellationToken ct = default);
    Task<Product?> GetByUpcAsync(string upc, CancellationToken ct = default);
    Task<Product?> GetProductWithCategoriesAsync(int id, CancellationToken ct = default);

    // Price history
    Task<ProductPriceHistory?> GetCurrentPriceHistoryAsync(int productId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductPriceHistory>> GetPriceHistoryAsync(int productId, CancellationToken ct = default);
    Task AddPriceHistoryAsync(ProductPriceHistory entry, CancellationToken ct = default);

    // Discount offers
    Task<IReadOnlyList<DiscountOffer>> GetDiscountOffersAsync(int productId, CancellationToken ct = default);
    Task<DiscountOffer?> GetDiscountOfferByIdAsync(int productId, int offerId, CancellationToken ct = default);
    Task AddDiscountOfferAsync(DiscountOffer offer, CancellationToken ct = default);
    void UpdateDiscountOffer(DiscountOffer offer);
    void DeleteDiscountOffer(DiscountOffer offer);

    // Images
    Task<IReadOnlyList<ProductImage>> GetImagesAsync(int productId, CancellationToken ct = default);
    Task<ProductImage?> GetImageByIdAsync(int productId, int imageId, CancellationToken ct = default);
    Task AddImageAsync(ProductImage image, CancellationToken ct = default);
    void UpdateImage(ProductImage image);
    void DeleteImage(ProductImage image);

    // Additional ShortIds
    Task<IReadOnlyList<ProductShortId>> GetShortIdsAsync(int productId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductShortId>> GetAllShortIdsAsync(bool? assigned = null, CancellationToken ct = default);
    Task<ProductShortId?> GetShortIdByIdAsync(int productId, int shortIdId, CancellationToken ct = default);
    Task<ProductShortId?> GetShortIdByIdAsync(int shortIdId, CancellationToken ct = default);
    Task<bool> ShortIdCodeExistsAsync(string code, CancellationToken ct = default);
    Task AddShortIdAsync(ProductShortId sid, CancellationToken ct = default);
    void UpdateShortId(ProductShortId sid);
    void DeleteShortId(ProductShortId sid);
}
