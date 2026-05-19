using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class ReviewService(IReviewRepository repo) : IReviewService
{
    public async Task<IReadOnlyList<ReviewDto>> GetByProductAsync(int productId, CancellationToken ct = default)
    {
        var reviews = await repo.GetByProductAsync(productId, ct);
        return reviews.Select(Map).ToList();
    }

    public async Task<double> GetAverageRatingAsync(int productId, CancellationToken ct = default)
    {
        var reviews = await repo.GetByProductAsync(productId, ct);
        if (reviews.Count == 0) return 0;
        return Math.Round(reviews.Average(r => r.Rating), 1);
    }

    public async Task<ReviewDto?> GetByProductAndUserAsync(int productId, string userId, CancellationToken ct = default)
    {
        var review = await repo.GetByProductAndUserAsync(productId, userId, ct);
        return review is null ? null : Map(review);
    }

    public async Task<ReviewDto> AddAsync(int productId, string userId, string userName, int rating, string? comment, CancellationToken ct = default)
    {
        var review = new Review
        {
            ProductId = productId,
            AppUserId = userId,
            UserName = userName,
            Rating = Math.Clamp(rating, 1, 5),
            Comment = comment?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(review, ct);
        await repo.SaveChangesAsync(ct);
        return Map(review);
    }

    private static ReviewDto Map(Review r) => new()
    {
        Id = r.Id,
        AppUserId = r.AppUserId,
        UserName = r.UserName,
        ProductId = r.ProductId,
        ProductName = r.Product?.Name ?? string.Empty,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt
    };
}
