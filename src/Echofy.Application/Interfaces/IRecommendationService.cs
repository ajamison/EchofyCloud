using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IRecommendationService
{
    Task<IReadOnlyList<RecommendationItemDto>> GetForUserAsync(
        string userId, int count = 8, int? clientId = null, CancellationToken ct = default);

    Task<IReadOnlyList<RecommendationItemDto>> GetPopularAsync(
        int count = 8, int? clientId = null, CancellationToken ct = default);

    Task<IReadOnlyList<RecommendationItemDto>> GetSimilarAsync(
        int productId, int count = 6, string? excludeUserId = null, CancellationToken ct = default);
}
