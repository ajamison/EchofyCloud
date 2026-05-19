using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class CustomerPortalController(
    IFavoriteService favoriteService,
    IReviewService reviewService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string UserName => User.FindFirstValue("fullName") ?? User.FindFirstValue(ClaimTypes.Email) ?? "";

    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites(CancellationToken ct)
        => Ok(await favoriteService.GetFavoritesAsync(UserId, ct));

    [HttpPost("favorites/{productId:int}/toggle")]
    public async Task<IActionResult> ToggleFavorite(int productId, CancellationToken ct)
    {
        var isFavorited = await favoriteService.ToggleFavoriteAsync(UserId, productId, ct);
        return Ok(new { isFavorited });
    }

    [HttpPost("reviews/{productId:int}")]
    public async Task<IActionResult> AddReview(int productId, [FromBody] AddReviewRequest req, CancellationToken ct)
    {
        var existing = await reviewService.GetByProductAndUserAsync(productId, UserId, ct);
        if (existing is not null)
            return Conflict(new { message = "You have already reviewed this product." });

        var review = await reviewService.AddAsync(productId, UserId, UserName, req.Rating, req.Comment, ct);
        return Ok(review);
    }
}

public record AddReviewRequest(int Rating, string? Comment);
