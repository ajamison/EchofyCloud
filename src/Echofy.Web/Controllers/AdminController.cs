using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Infrastructure.Data;
using Echofy.Infrastructure.Identity;
using Echofy.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin,SuperUser")]
public class AdminController(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext db,
    IReferralService referralService) : Controller
{
    // ── Users ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Users()
    {
        ViewData["ActivePage"] = "AdminUsers";
        var users = await userManager.Users
            .Include(u => u.Client)
            .OrderBy(u => u.Email)
            .ToListAsync();

        var models = new List<UserListItemViewModel>();
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u);
            models.Add(new UserListItemViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                Role = roles.FirstOrDefault() ?? "—",
                ClientName = u.Client?.Name ?? "—"
            });
        }

        return View(models);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        ViewData["ActivePage"] = "AdminUsers";
        var user = await userManager.Users.Include(u => u.Client).FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        var allRoles = roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
        var allClients = await db.Clients.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

        var vm = new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            SelectedRole = roles.FirstOrDefault() ?? "",
            ClientId = user.ClientId,
            RoleOptions = allRoles.Select(r => new SelectListItem(r, r)).ToList(),
            ClientOptions = allClients.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
        };

        return View(vm);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(EditUserViewModel vm)
    {
        var user = await userManager.FindByIdAsync(vm.Id);
        if (user is null) return NotFound();

        user.FullName = vm.FullName;
        user.ClientId = vm.ClientId;
        await userManager.UpdateAsync(user);

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!string.IsNullOrEmpty(vm.SelectedRole))
            await userManager.AddToRoleAsync(user, vm.SelectedRole);

        return RedirectToAction(nameof(Users));
    }

    // ── Create User ────────────────────────────────────────────────────────

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> CreateUser()
    {
        ViewData["ActivePage"] = "AdminUsers";
        var allRoles = roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
        var allClients = await db.Clients.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

        var vm = new CreateUserViewModel
        {
            RoleOptions = allRoles.Select(r => new SelectListItem(r, r)).ToList(),
            ClientOptions = allClients.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
        };

        return View(vm);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var allRoles = roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
            var allClients = await db.Clients.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            vm.RoleOptions = allRoles.Select(r => new SelectListItem(r, r)).ToList();
            vm.ClientOptions = allClients.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            return View(vm);
        }

        var user = new AppUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FullName = vm.FullName,
            ClientId = vm.ClientId
        };

        var result = await userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);

            var allRoles = roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToList();
            var allClients = await db.Clients.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
            vm.RoleOptions = allRoles.Select(r => new SelectListItem(r, r)).ToList();
            vm.ClientOptions = allClients.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            return View(vm);
        }

        if (!string.IsNullOrEmpty(vm.SelectedRole))
            await userManager.AddToRoleAsync(user, vm.SelectedRole);

        return RedirectToAction(nameof(Users));
    }

    // ── Clients ────────────────────────────────────────────────────────────

    public async Task<IActionResult> Clients()
    {
        ViewData["ActivePage"] = "AdminClients";
        var clients = await db.Clients.OrderBy(c => c.Name).ToListAsync();
        return View(clients);
    }

    [HttpGet]
    public async Task<IActionResult> EditClient(int id)
    {
        ViewData["ActivePage"] = "AdminClients";
        var client = await db.Clients.FindAsync(id);
        if (client is null) return NotFound();

        var vm = new EditClientViewModel
        {
            Id = client.Id,
            Name = client.Name,
            Slug = client.Slug,
            HasECommerce = client.HasECommerce,
            HasCrm = client.HasCrm,
            HasKanban = client.HasKanban,
            HasCalendar = client.HasCalendar,
            HasChat = client.HasChat,
            IsActive = client.IsActive
        };

        return View(vm);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClient(EditClientViewModel vm)
    {
        var client = await db.Clients.FindAsync(vm.Id);
        if (client is null) return NotFound();

        client.Name = vm.Name;
        client.Slug = vm.Slug;
        client.HasECommerce = vm.HasECommerce;
        client.HasCrm = vm.HasCrm;
        client.HasKanban = vm.HasKanban;
        client.HasCalendar = vm.HasCalendar;
        client.HasChat = vm.HasChat;
        client.IsActive = vm.IsActive;

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Clients));
    }

    // ── Categories ─────────────────────────────────────────────────────────

    public async Task<IActionResult> Categories()
    {
        ViewData["ActivePage"] = "AdminCategories";
        var categories = await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        return View(categories);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public IActionResult CreateCategory()
    {
        ViewData["ActivePage"] = "AdminCategories";
        return View(new CategoryViewModel { IsActive = true });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CategoryViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        db.Categories.Add(new Category
        {
            Name = vm.Name,
            Slug = vm.Slug,
            Description = vm.Description,
            IsActive = vm.IsActive
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Categories));
    }

    [HttpGet]
    public async Task<IActionResult> EditCategory(int id)
    {
        ViewData["ActivePage"] = "AdminCategories";
        var cat = await db.Categories
            .Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cat is null) return NotFound();
        return View(cat);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var cat = await db.Categories.FindAsync(vm.Id);
        if (cat is null) return NotFound();

        cat.Name = vm.Name;
        cat.Slug = vm.Slug;
        cat.Description = vm.Description;
        cat.IsActive = vm.IsActive;

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Categories));
    }

    // ── Create Client ──────────────────────────────────────────────────────

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public IActionResult CreateClient()
    {
        ViewData["ActivePage"] = "AdminClients";
        return View(new EditClientViewModel { IsActive = true });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(EditClientViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var client = new Client
        {
            Name = vm.Name,
            Slug = vm.Slug,
            HasECommerce = vm.HasECommerce,
            HasCrm = vm.HasCrm,
            HasKanban = vm.HasKanban,
            HasCalendar = vm.HasCalendar,
            HasChat = vm.HasChat,
            IsActive = vm.IsActive
        };

        db.Clients.Add(client);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Clients));
    }

    // ── Manufacturers ──────────────────────────────────────────────────────

    public async Task<IActionResult> Manufacturers()
    {
        ViewData["ActivePage"] = "AdminManufacturers";
        var manufacturers = await db.Manufacturers.OrderBy(m => m.Name).ToListAsync();
        return View(manufacturers);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public IActionResult CreateManufacturer()
    {
        ViewData["ActivePage"] = "AdminManufacturers";
        return View(new ManufacturerViewModel { IsActive = true });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateManufacturer(ManufacturerViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        db.Manufacturers.Add(new Manufacturer
        {
            Name = vm.Name,
            Website = vm.Website,
            IsActive = vm.IsActive
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Manufacturers));
    }

    [HttpGet]
    public async Task<IActionResult> EditManufacturer(int id)
    {
        ViewData["ActivePage"] = "AdminManufacturers";
        var m = await db.Manufacturers.FindAsync(id);
        if (m is null) return NotFound();

        return View(new ManufacturerViewModel
        {
            Id = m.Id,
            Name = m.Name,
            Website = m.Website,
            IsActive = m.IsActive
        });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditManufacturer(ManufacturerViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var m = await db.Manufacturers.FindAsync(vm.Id);
        if (m is null) return NotFound();

        m.Name = vm.Name;
        m.Website = vm.Website;
        m.IsActive = vm.IsActive;

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Manufacturers));
    }

    // ── Units of Measure ───────────────────────────────────────────────────

    public async Task<IActionResult> UnitsOfMeasure()
    {
        ViewData["ActivePage"] = "AdminUnitsOfMeasure";
        var units = await db.UnitsOfMeasure.OrderBy(u => u.Name).ToListAsync();
        return View(units);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public IActionResult CreateUnitOfMeasure()
    {
        ViewData["ActivePage"] = "AdminUnitsOfMeasure";
        return View(new UnitOfMeasureViewModel { IsActive = true });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUnitOfMeasure(UnitOfMeasureViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        db.UnitsOfMeasure.Add(new UnitOfMeasure
        {
            Name = vm.Name,
            Abbreviation = vm.Abbreviation,
            IsActive = vm.IsActive
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(UnitsOfMeasure));
    }

    [HttpGet]
    public async Task<IActionResult> EditUnitOfMeasure(int id)
    {
        ViewData["ActivePage"] = "AdminUnitsOfMeasure";
        var u = await db.UnitsOfMeasure.FindAsync(id);
        if (u is null) return NotFound();

        return View(new UnitOfMeasureViewModel
        {
            Id = u.Id,
            Name = u.Name,
            Abbreviation = u.Abbreviation,
            IsActive = u.IsActive
        });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUnitOfMeasure(UnitOfMeasureViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var u = await db.UnitsOfMeasure.FindAsync(vm.Id);
        if (u is null) return NotFound();

        u.Name = vm.Name;
        u.Abbreviation = vm.Abbreviation;
        u.IsActive = vm.IsActive;

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(UnitsOfMeasure));
    }

    // ── Reviews ────────────────────────────────────────────────────────────

    public async Task<IActionResult> Reviews()
    {
        ViewData["ActivePage"] = "AdminReviews";
        var reviews = await db.Reviews
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reviews);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await db.Reviews.FindAsync(id);
        if (review is not null)
        {
            db.Reviews.Remove(review);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Reviews));
    }

    // ── Referrals ──────────────────────────────────────────────────────────

    public async Task<IActionResult> Referrals()
    {
        ViewData["ActivePage"] = "AdminReferrals";
        var referrals = await referralService.GetAllForAdminAsync();

        // Enrich with referrer name/email from Identity
        var userIds = referrals.Select(r => r.ReferrerId).Distinct().ToList();
        var users = await userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var enriched = referrals.Select(r =>
        {
            if (users.TryGetValue(r.ReferrerId, out var u))
            {
                r.ReferrerEmail = u.Email ?? r.ReferrerId;
                r.ReferrerName = u.FullName;
            }
            return r;
        }).ToList();

        return View(enriched);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReferralReward(int id)
    {
        await referralService.ApproveRewardAsync(id);
        return RedirectToAction(nameof(Referrals));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> IssueReferralReward(int id)
    {
        await referralService.IssueRewardAsync(id);
        return RedirectToAction(nameof(Referrals));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReferralReward(int id)
    {
        await referralService.CancelRewardAsync(id);
        return RedirectToAction(nameof(Referrals));
    }
}
