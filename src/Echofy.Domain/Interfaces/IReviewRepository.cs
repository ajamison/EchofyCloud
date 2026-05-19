using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetByProductAsync(int productId, CancellationToken ct = default);
    Task<Review?> GetByProductAndUserAsync(int productId, string userId, CancellationToken ct = default);
    Task AddAsync(Review review, CancellationToken ct = default);
    void Delete(Review review);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
