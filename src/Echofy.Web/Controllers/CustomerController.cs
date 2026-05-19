using System.Security.Claims;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class CustomerController(
    IFavoriteService favoriteService,
    IRecommendationService recommendationService,
    IReferralService referralService) : Controller
{
    [HttpGet("/customer/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        ViewData["ActivePage"] = "CustomerDashboard";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var clientId = GetClientId();

        var favoritesTask = favoriteService.GetFavoritesAsync(userId);
        var recsTask = recommendationService.GetForUserAsync(userId, 8, clientId);
        var referralTask = referralService.GetOrCreateReferralAsync(userId, GetBaseUrl());

        await Task.WhenAll(favoritesTask, recsTask, referralTask);

        var favorites = favoritesTask.Result;
        ViewBag.FavoritesCount = favorites.Count;
        ViewBag.RecentFavorites = favorites.Take(3).ToList();
        ViewBag.Recommendations = recsTask.Result;
        ViewBag.Referral = referralTask.Result;
        return View();
    }

    [HttpGet("/customer/favorites")]
    public async Task<IActionResult> Favorites()
    {
        ViewData["ActivePage"] = "CustomerFavorites";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var favorites = await favoriteService.GetFavoritesAsync(userId);
        return View(favorites);
    }

    [HttpGet("/customer/referrals")]
    public async Task<IActionResult> Referrals()
    {
        ViewData["ActivePage"] = "CustomerReferrals";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var referral = await referralService.GetOrCreateReferralAsync(userId, GetBaseUrl());
        return View(referral);
    }

    private int? GetClientId()
    {
        var raw = User.FindFirstValue("echofy:client:id");
        return int.TryParse(raw, out var id) ? id : null;
    }

    private string GetBaseUrl()
        => $"{Request.Scheme}://{Request.Host}";
}
