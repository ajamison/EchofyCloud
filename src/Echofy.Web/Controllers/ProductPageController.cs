using System.Security.Claims;
using Echofy.Application.Interfaces;
using Echofy.Infrastructure.Identity;
using Echofy.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

/// <summary>
/// Public-facing product page — no authentication required to view.
/// Reviews and favorites require authentication.
/// </summary>
public class ProductPageController(
    IProductService productService,
    IReviewService reviewService,
    IFavoriteService favoriteService,
    IRecommendationService recommendationService,
    UserManager<AppUser> userManager) : Controller
{
    [HttpGet("/p/{shortId}")]
    public async Task<IActionResult> Index(string shortId)
    {
        var product = await productService.GetByShortIdAsync(shortId);
        if (product is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var reviewsTask = reviewService.GetByProductAsync(product.Id);
        var avgRatingTask = reviewService.GetAverageRatingAsync(product.Id);
        var similarTask = recommendationService.GetSimilarAsync(product.Id, 6, userId);

        Task<bool> favTask = Task.FromResult(false);
        Task<Application.DTOs.ReviewDto?> reviewedTask =
            Task.FromResult<Application.DTOs.ReviewDto?>(null);

        if (userId is not null)
        {
            favTask = favoriteService.IsFavoritedAsync(userId, product.Id);
            reviewedTask = reviewService.GetByProductAndUserAsync(product.Id, userId);
        }

        await Task.WhenAll(reviewsTask, avgRatingTask, similarTask, favTask, reviewedTask);

        var vm = new ProductPageViewModel
        {
            Product = product,
            Reviews = reviewsTask.Result,
            AverageRating = avgRatingTask.Result,
            IsFavorited = favTask.Result,
            UserHasReviewed = reviewedTask.Result is not null
        };

        ViewBag.SimilarProducts = similarTask.Result;
        return View(vm);
    }

    [HttpPost("/p/{shortId}/review"), Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(string shortId, int rating, string? comment)
    {
        var product = await productService.GetByShortIdAsync(shortId);
        if (product is null) return NotFound();

        var user = await userManager.GetUserAsync(User);
        if (user is null) return Forbid();

        // Guard: already reviewed
        var existing = await reviewService.GetByProductAndUserAsync(product.Id, user.Id);
        if (existing is null && rating >= 1 && rating <= 5)
        {
            await reviewService.AddAsync(product.Id, user.Id, user.FullName.Length > 0 ? user.FullName : user.Email!, rating, comment);
        }

        return Redirect($"/p/{shortId}#reviews");
    }

    [HttpPost("/p/{shortId}/favorite"), Authorize]
    public async Task<IActionResult> ToggleFavorite(string shortId)
    {
        var product = await productService.GetByShortIdAsync(shortId);
        if (product is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var nowFavorited = await favoriteService.ToggleFavoriteAsync(userId, product.Id);

        return Json(new { favorited = nowFavorited });
    }
}
