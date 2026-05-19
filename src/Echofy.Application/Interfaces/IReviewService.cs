using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetByProductAsync(int productId, CancellationToken ct = default);
    Task<double> GetAverageRatingAsync(int productId, CancellationToken ct = default);
    Task<ReviewDto?> GetByProductAndUserAsync(int productId, string userId, CancellationToken ct = default);
    Task<ReviewDto> AddAsync(int productId, string userId, string userName, int rating, string? comment, CancellationToken ct = default);
}
