using Echofy.RecommendationApi.Data;
using Echofy.RecommendationApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Echofy.RecommendationApi.Services;

public class RecommendationEngine(RecommendationDbContext db)
{
    // Returns productIds that a user has favorited OR rated >= 4
    private async Task<HashSet<int>> BuildLikedSetAsync(string userId, CancellationToken ct)
    {
        var favIds = await db.FavoriteProducts
            .AsNoTracking()
            .Where(f => f.AppUserId == userId)
            .Select(f => f.ProductId)
            .ToListAsync(ct);

        var ratedIds = await db.Reviews
            .AsNoTracking()
            .Where(r => r.AppUserId == userId && r.Rating >= 4)
            .Select(r => r.ProductId)
            .ToListAsync(ct);

        return [.. favIds, .. ratedIds];
    }

    // Resolve image URL for a product (checks uploaded images first, falls back to ImageUrl)
    private static string? ResolveImageUrl(
        int productId,
        string? productImageUrl,
        Dictionary<int, List<(string FileName, bool IsMain)>> imagesByProduct)
    {
        if (imagesByProduct.TryGetValue(productId, out var imgs) && imgs.Count > 0)
        {
            var main = imgs.FirstOrDefault(i => i.IsMain);
            var img = main != default ? main : imgs[0];
            return $"/uploads/images/{img.FileName}";
        }
        return productImageUrl;
    }

    public async Task<List<RecommendationItemDto>> GetForUserAsync(
        string userId, int count, int? clientId, CancellationToken ct)
    {
        var likedSet = await BuildLikedSetAsync(userId, ct);
        if (likedSet.Count == 0)
            return await GetPopularAsync(count, clientId, ct);

        // Load ALL favorites and high-rated reviews grouped by userId
        var allFavs = await db.FavoriteProducts
            .AsNoTracking()
            .Select(f => new { f.AppUserId, f.ProductId })
            .ToListAsync(ct);

        var allHighRatings = await db.Reviews
            .AsNoTracking()
            .Where(r => r.Rating >= 4)
            .Select(r => new { r.AppUserId, r.ProductId })
            .ToListAsync(ct);

        // Build liked sets for all users
        var userLikedSets = new Dictionary<string, HashSet<int>>();
        foreach (var fav in allFavs)
        {
            if (!userLikedSets.TryGetValue(fav.AppUserId, out var set))
                userLikedSets[fav.AppUserId] = set = [];
            set.Add(fav.ProductId);
        }
        foreach (var r in allHighRatings)
        {
            if (!userLikedSets.TryGetValue(r.AppUserId, out var set))
                userLikedSets[r.AppUserId] = set = [];
            set.Add(r.ProductId);
        }

        // Score unseen products via Jaccard similarity
        var scores = new Dictionary<int, double>();
        foreach (var (otherUserId, otherSet) in userLikedSets)
        {
            if (otherUserId == userId) continue;
            int intersection = likedSet.Count(id => otherSet.Contains(id));
            if (intersection == 0) continue;
            int union = likedSet.Count + otherSet.Count - intersection;
            double jaccard = (double)intersection / union;

            foreach (var pid in otherSet)
            {
                if (likedSet.Contains(pid)) continue;
                scores.TryGetValue(pid, out double existing);
                scores[pid] = existing + jaccard;
            }
        }

        if (scores.Count == 0)
            return await GetPopularAsync(count, clientId, ct);

        var topIds = scores
            .OrderByDescending(kv => kv.Value)
            .Take(count * 2)
            .Select(kv => kv.Key)
            .ToList();

        var query = db.Products
            .AsNoTracking()
            .Where(p => topIds.Contains(p.Id) && p.IsActive);

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        var products = await query.ToListAsync(ct);

        var imagesByProduct = await GetImagesByProductAsync([.. products.Select(p => p.Id)], ct);

        return products
            .Where(p => scores.ContainsKey(p.Id))
            .OrderByDescending(p => scores[p.Id])
            .Take(count)
            .Select(p => new RecommendationItemDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Price = p.Price,
                ShortId = p.ShortId,
                ImageUrl = ResolveImageUrl(p.Id, p.ImageUrl, imagesByProduct),
                Score = scores[p.Id],
                Reason = "Recommended for you"
            })
            .ToList();
    }

    public async Task<List<RecommendationItemDto>> GetPopularAsync(
        int count, int? clientId, CancellationToken ct)
    {
        var query = db.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (clientId.HasValue)
            query = query.Where(p => p.ClientId == clientId.Value);

        var products = await query.ToListAsync(ct);
        if (products.Count == 0) return [];

        var productIds = products.Select(p => p.Id).ToList();

        // Load review averages
        var reviewStats = await db.Reviews
            .AsNoTracking()
            .Where(r => productIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Avg = g.Average(r => (double)r.Rating) })
            .ToListAsync(ct);

        var avgByProduct = reviewStats.ToDictionary(r => r.ProductId, r => r.Avg);

        // Load favorite counts
        var favCounts = await db.FavoriteProducts
            .AsNoTracking()
            .Where(f => productIds.Contains(f.ProductId))
            .GroupBy(f => f.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var favCountByProduct = favCounts.ToDictionary(f => f.ProductId, f => f.Count);
        int maxFavCount = favCountByProduct.Values.DefaultIfEmpty(1).Max();

        var imagesByProduct = await GetImagesByProductAsync(productIds, ct);

        return products
            .Select(p =>
            {
                double avgRating = avgByProduct.TryGetValue(p.Id, out var avg) ? avg : 0;
                int favCount = favCountByProduct.TryGetValue(p.Id, out var fc) ? fc : 0;
                double score = (avgRating / 5.0) * 0.6 + ((double)favCount / maxFavCount) * 0.4;
                return (Product: p, Score: score);
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => new RecommendationItemDto
            {
                ProductId = x.Product.Id,
                Name = x.Product.Name,
                Price = x.Product.Price,
                ShortId = x.Product.ShortId,
                ImageUrl = ResolveImageUrl(x.Product.Id, x.Product.ImageUrl, imagesByProduct),
                Score = x.Score,
                Reason = "Popular right now"
            })
            .ToList();
    }

    public async Task<List<RecommendationItemDto>> GetSimilarAsync(
        int productId, int count, string? excludeUserId, CancellationToken ct)
    {
        // Load target product with categories
        var target = await db.Products
            .AsNoTracking()
            .Include(p => p.Categories)
            .FirstOrDefaultAsync(p => p.Id == productId, ct);

        if (target is null) return [];

        var categoryIds = target.Categories.Select(c => c.Id).ToList();

        // Find products sharing any category or same manufacturer
        var candidateQuery = db.Products
            .AsNoTracking()
            .Include(p => p.Categories)
            .Where(p => p.Id != productId && p.IsActive)
            .Where(p =>
                p.Categories.Any(c => categoryIds.Contains(c.Id)) ||
                (target.ManufacturerId != null && p.ManufacturerId == target.ManufacturerId));

        var candidates = await candidateQuery.ToListAsync(ct);
        if (candidates.Count == 0) return [];

        var candidateIds = candidates.Select(p => p.Id).ToList();

        // Exclude products already liked by the user
        HashSet<int> excludeIds = [];
        if (excludeUserId is not null)
            excludeIds = await BuildLikedSetAsync(excludeUserId, ct);

        // Load review averages and favorite counts for candidates
        var reviewStats = await db.Reviews
            .AsNoTracking()
            .Where(r => candidateIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Avg = g.Average(r => (double)r.Rating) })
            .ToListAsync(ct);

        var avgByProduct = reviewStats.ToDictionary(r => r.ProductId, r => r.Avg);

        var favCounts = await db.FavoriteProducts
            .AsNoTracking()
            .Where(f => candidateIds.Contains(f.ProductId))
            .GroupBy(f => f.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var favCountByProduct = favCounts.ToDictionary(f => f.ProductId, f => f.Count);
        int maxFavCount = favCountByProduct.Values.DefaultIfEmpty(1).Max();

        var imagesByProduct = await GetImagesByProductAsync(candidateIds, ct);

        return candidates
            .Where(p => !excludeIds.Contains(p.Id))
            .Select(p =>
            {
                double avgRating = avgByProduct.TryGetValue(p.Id, out var avg) ? avg : 0;
                int favCount = favCountByProduct.TryGetValue(p.Id, out var fc) ? fc : 0;
                double score = (avgRating / 5.0) * 0.6 + ((double)favCount / maxFavCount) * 0.4;
                return (Product: p, Score: score);
            })
            .OrderByDescending(x => x.Score)
            .Take(count)
            .Select(x => new RecommendationItemDto
            {
                ProductId = x.Product.Id,
                Name = x.Product.Name,
                Price = x.Product.Price,
                ShortId = x.Product.ShortId,
                ImageUrl = ResolveImageUrl(x.Product.Id, x.Product.ImageUrl, imagesByProduct),
                Score = x.Score,
                Reason = "Similar products"
            })
            .ToList();
    }

    private async Task<Dictionary<int, List<(string FileName, bool IsMain)>>> GetImagesByProductAsync(
        List<int> productIds, CancellationToken ct)
    {
        var images = await db.ProductImages
            .AsNoTracking()
            .Where(i => productIds.Contains(i.ProductId))
            .Select(i => new { i.ProductId, i.FileName, i.IsMain })
            .ToListAsync(ct);

        return images
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(i => (i.FileName, i.IsMain)).ToList());
    }
}
