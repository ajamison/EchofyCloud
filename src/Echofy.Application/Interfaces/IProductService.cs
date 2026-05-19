using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IProductService
{
    // clientId = tenant scope from claims; companyId = optional company-specific filter
    Task<IReadOnlyList<ProductDto>> GetAllAsync(int? clientId = null, string? search = null, bool? activeOnly = null, CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ProductDto?> GetByShortIdAsync(string shortId, CancellationToken ct = default);
    Task<ProductDto?> GetByUpcAsync(string upc, CancellationToken ct = default);
    Task<ProductDto?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(ProductDto dto, string? changedByUserId = null, CancellationToken ct = default);
    Task<ProductDto?> UpdateAsync(int id, ProductDto dto, int? clientId = null, string? changedByUserId = null, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, int? clientId = null, CancellationToken ct = default);

    Task<IReadOnlyList<DiscountOfferDto>> GetDiscountOffersAsync(int productId, CancellationToken ct = default);
    Task<DiscountOfferDto> CreateDiscountOfferAsync(int productId, DiscountOfferDto dto, CancellationToken ct = default);
    Task<DiscountOfferDto?> UpdateDiscountOfferAsync(int productId, int offerId, DiscountOfferDto dto, CancellationToken ct = default);
    Task<bool> DeleteDiscountOfferAsync(int productId, int offerId, CancellationToken ct = default);

    Task<ProductImageDto> AddImageAsync(int productId, string fileName, string? altText, string? sku = null, CancellationToken ct = default);
    Task<bool> DeleteImageAsync(int productId, int imageId, CancellationToken ct = default);
    Task<bool> SetMainImageAsync(int productId, int imageId, CancellationToken ct = default);

    Task<ProductShortIdDto> GenerateAdditionalShortIdAsync(int productId, string? label, CancellationToken ct = default);
    Task<bool> DeleteAdditionalShortIdAsync(int productId, int shortIdId, CancellationToken ct = default);

    // Unassigned label pool
    Task<IReadOnlyList<ProductShortIdDto>> GetAllShortIdsAsync(bool? assigned = null, CancellationToken ct = default);
    Task<IReadOnlyList<ProductShortIdDto>> GenerateBatchShortIdsAsync(int count, string? label, CancellationToken ct = default);
    Task<ProductShortIdDto?> AssignProductAsync(int shortIdId, int productId, CancellationToken ct = default);
    Task<bool> UnassignShortIdAsync(int shortIdId, CancellationToken ct = default);
    Task<bool> DeleteShortIdAsync(int shortIdId, CancellationToken ct = default);
}
