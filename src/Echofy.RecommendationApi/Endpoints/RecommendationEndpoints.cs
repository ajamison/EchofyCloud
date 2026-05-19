using Echofy.RecommendationApi.DTOs;
using Echofy.RecommendationApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.RecommendationApi.Endpoints;

public static class RecommendationEndpoints
{
    public static void MapRecommendationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recommendations");

        group.MapGet("/user/{userId}", async (
            string userId,
            [FromQuery] int count = 8,
            [FromQuery] int? clientId = null,
            RecommendationEngine engine = default!,
            CancellationToken ct = default) =>
        {
            var items = await engine.GetForUserAsync(userId, count, clientId, ct);
            return Results.Ok(new RecommendationResponseDto { Items = items });
        });

        group.MapGet("/popular", async (
            [FromQuery] int count = 8,
            [FromQuery] int? clientId = null,
            RecommendationEngine engine = default!,
            CancellationToken ct = default) =>
        {
            var items = await engine.GetPopularAsync(count, clientId, ct);
            return Results.Ok(new RecommendationResponseDto { Items = items });
        });

        group.MapGet("/similar/{productId:int}", async (
            int productId,
            [FromQuery] int count = 6,
            [FromQuery] string? excludeUserId = null,
            RecommendationEngine engine = default!,
            CancellationToken ct = default) =>
        {
            var items = await engine.GetSimilarAsync(productId, count, excludeUserId, ct);
            return Results.Ok(new RecommendationResponseDto { Items = items });
        });
    }
}
