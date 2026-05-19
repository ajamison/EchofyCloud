using Echofy.Application.Interfaces;
using Echofy.Infrastructure.Identity;
using Echofy.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

public class AccountController(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    IReferralService referralService) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? Url.Action("Index", "Dashboard")!);

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register(string? @ref = null)
    {
        return View(new RegisterViewModel { ReferralCode = @ref });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(model.ReferralCode))
            {
                var welcomeCoupon = await referralService.UseReferralCodeAsync(
                    model.ReferralCode.Trim().ToUpperInvariant(), user.Id, user.Email!, default);

                if (welcomeCoupon is not null)
                    TempData["WelcomeCoupon"] = welcomeCoupon;
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Dashboard", "Customer");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
