using System.Net.Http.Json;
using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Echofy.Infrastructure.Services;

public class RecommendationHttpService(
    HttpClient http,
    ILogger<RecommendationHttpService> logger) : IRecommendationService
{
    public async Task<IReadOnlyList<RecommendationItemDto>> GetForUserAsync(
        string userId, int count = 8, int? clientId = null, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/recommendations/user/{Uri.EscapeDataString(userId)}?count={count}"
                      + (clientId.HasValue ? $"&clientId={clientId.Value}" : "");
            var response = await http.GetFromJsonAsync<RecommendationResponse>(url, ct);
            return response?.Items ?? [];
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Recommendation API unavailable (GetForUser)");
            return [];
        }
    }

    public async Task<IReadOnlyList<RecommendationItemDto>> GetPopularAsync(
        int count = 8, int? clientId = null, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/recommendations/popular?count={count}"
                      + (clientId.HasValue ? $"&clientId={clientId.Value}" : "");
            var response = await http.GetFromJsonAsync<RecommendationResponse>(url, ct);
            return response?.Items ?? [];
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Recommendation API unavailable (GetPopular)");
            return [];
        }
    }

    public async Task<IReadOnlyList<RecommendationItemDto>> GetSimilarAsync(
        int productId, int count = 6, string? excludeUserId = null, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/recommendations/similar/{productId}?count={count}"
                      + (excludeUserId is not null
                          ? $"&excludeUserId={Uri.EscapeDataString(excludeUserId)}"
                          : "");
            var response = await http.GetFromJsonAsync<RecommendationResponse>(url, ct);
            return response?.Items ?? [];
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Recommendation API unavailable (GetSimilar)");
            return [];
        }
    }

    // Local DTO matching the API response shape
    private sealed class RecommendationResponse
    {
        public List<RecommendationItemDto> Items { get; set; } = [];
    }
}
