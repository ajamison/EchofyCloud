using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class ReviewRepository(ApplicationDbContext db) : IReviewRepository
{
    public async Task<IReadOnlyList<Review>> GetByProductAsync(int productId, CancellationToken ct = default)
        => await db.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<Review?> GetByProductAndUserAsync(int productId, string userId, CancellationToken ct = default)
        => await db.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.AppUserId == userId, ct);

    public async Task AddAsync(Review review, CancellationToken ct = default)
        => await db.Reviews.AddAsync(review, ct);

    public void Delete(Review review)
        => db.Reviews.Remove(review);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
