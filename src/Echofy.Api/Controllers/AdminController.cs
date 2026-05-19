using Echofy.Domain.Entities;
using Echofy.Infrastructure.Data;
using Echofy.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,SuperAdmin,SuperUser")]
public class AdminController(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ApplicationDbContext db) : ControllerBase
{
    // ── Users ──────────────────────────────────────────────────────────────

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userManager.Users.Include(u => u.Client).OrderBy(u => u.Email).ToListAsync();
        var result = new List<object>();
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u);
            result.Add(new
            {
                u.Id, u.FullName, u.Email,
                Role       = roles.FirstOrDefault() ?? "",
                u.ClientId,
                ClientName = u.Client?.Name
            });
        }
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await userManager.Users.Include(u => u.Client).FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();
        var roles = await userManager.GetRolesAsync(user);
        return Ok(new { user.Id, user.FullName, user.Email, Role = roles.FirstOrDefault() ?? "", user.ClientId, ClientName = user.Client?.Name });
    }

    [HttpPost("users")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
    {
        var user = new AppUser { UserName = req.Email, Email = req.Email, FullName = req.FullName, ClientId = req.ClientId };
        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        if (!string.IsNullOrEmpty(req.Role))
            await userManager.AddToRoleAsync(user, req.Role);

        return Ok(new { user.Id });
    }

    [HttpPut("users/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest req)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        user.FullName = req.FullName;
        user.ClientId = req.ClientId;
        await userManager.UpdateAsync(user);

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!string.IsNullOrEmpty(req.Role))
            await userManager.AddToRoleAsync(user, req.Role);

        return NoContent();
    }

    [HttpPost("users/{id}/reset-password")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest req)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var remove = await userManager.RemovePasswordAsync(user);
        if (!remove.Succeeded)
            return BadRequest(remove.Errors.Select(e => e.Description));

        var add = await userManager.AddPasswordAsync(user, req.NewPassword);
        if (!add.Succeeded)
            return BadRequest(add.Errors.Select(e => e.Description));

        return NoContent();
    }

    [HttpGet("roles")]
    public IActionResult GetRoles()
        => Ok(roleManager.Roles.Select(r => r.Name).OrderBy(r => r).ToList());

    // ── Clients ────────────────────────────────────────────────────────────

    [HttpGet("clients")]
    public async Task<IActionResult> GetClients()
        => Ok(await db.Clients.OrderBy(c => c.Name).ToListAsync());

    [HttpGet("clients/{id:int}")]
    public async Task<IActionResult> GetClient(int id)
    {
        var client = await db.Clients.FindAsync(id);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost("clients")]
    public async Task<IActionResult> CreateClient([FromBody] ClientRequest req)
    {
        var client = new Client
        {
            Name = req.Name, Slug = req.Slug,
            HasECommerce = req.HasECommerce, HasCrm = req.HasCrm,
            HasKanban = req.HasKanban, HasCalendar = req.HasCalendar,
            HasChat = req.HasChat, IsActive = req.IsActive,
            AllowCompanyRewardOverride = req.AllowCompanyRewardOverride,
        };
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        return Ok(client);
    }

    [HttpPut("clients/{id:int}")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientRequest req)
    {
        var client = await db.Clients.FindAsync(id);
        if (client is null) return NotFound();
        client.Name = req.Name; client.Slug = req.Slug;
        client.HasECommerce = req.HasECommerce; client.HasCrm = req.HasCrm;
        client.HasKanban = req.HasKanban; client.HasCalendar = req.HasCalendar;
        client.HasChat = req.HasChat; client.IsActive = req.IsActive;
        client.AllowCompanyRewardOverride = req.AllowCompanyRewardOverride;
        await db.SaveChangesAsync();
        return Ok(client);
    }

    // ── Categories ─────────────────────────────────────────────────────────

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
        => Ok(await db.Categories.OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Slug, c.Description, c.IsActive, ProductCount = c.Products.Count })
            .ToListAsync());

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest req)
    {
        var cat = new Category { Name = req.Name, Slug = req.Slug, Description = req.Description, IsActive = req.IsActive };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();
        return Ok(cat);
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryRequest req)
    {
        var cat = await db.Categories.FindAsync(id);
        if (cat is null) return NotFound();
        cat.Name = req.Name; cat.Slug = req.Slug; cat.Description = req.Description; cat.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(cat);
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var cat = await db.Categories.FindAsync(id);
        if (cat is null) return NotFound();
        db.Categories.Remove(cat);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Manufacturers ──────────────────────────────────────────────────────

    [HttpGet("manufacturers")]
    public async Task<IActionResult> GetManufacturers()
        => Ok(await db.Manufacturers.OrderBy(m => m.Name).ToListAsync());

    [HttpPost("manufacturers")]
    public async Task<IActionResult> CreateManufacturer([FromBody] ManufacturerRequest req)
    {
        var m = new Manufacturer { Name = req.Name, Website = req.Website, IsActive = req.IsActive };
        db.Manufacturers.Add(m);
        await db.SaveChangesAsync();
        return Ok(m);
    }

    [HttpPut("manufacturers/{id:int}")]
    public async Task<IActionResult> UpdateManufacturer(int id, [FromBody] ManufacturerRequest req)
    {
        var m = await db.Manufacturers.FindAsync(id);
        if (m is null) return NotFound();
        m.Name = req.Name; m.Website = req.Website; m.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(m);
    }

    [HttpDelete("manufacturers/{id:int}")]
    public async Task<IActionResult> DeleteManufacturer(int id)
    {
        var m = await db.Manufacturers.FindAsync(id);
        if (m is null) return NotFound();
        db.Manufacturers.Remove(m);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Units of Measure ───────────────────────────────────────────────────

    [HttpGet("units")]
    public async Task<IActionResult> GetUnits()
        => Ok(await db.UnitsOfMeasure.OrderBy(u => u.Name).ToListAsync());

    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit([FromBody] UnitRequest req)
    {
        var u = new UnitOfMeasure { Name = req.Name, Abbreviation = req.Abbreviation, IsActive = req.IsActive };
        db.UnitsOfMeasure.Add(u);
        await db.SaveChangesAsync();
        return Ok(u);
    }

    [HttpPut("units/{id:int}")]
    public async Task<IActionResult> UpdateUnit(int id, [FromBody] UnitRequest req)
    {
        var u = await db.UnitsOfMeasure.FindAsync(id);
        if (u is null) return NotFound();
        u.Name = req.Name; u.Abbreviation = req.Abbreviation; u.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(u);
    }

    [HttpDelete("units/{id:int}")]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        var u = await db.UnitsOfMeasure.FindAsync(id);
        if (u is null) return NotFound();
        db.UnitsOfMeasure.Remove(u);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Companies ──────────────────────────────────────────────────────────

    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies()
        => Ok(await db.Companies
            .Include(c => c.Client)
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id, c.ClientId,
                ClientName   = c.Client != null ? c.Client.Name : null,
                c.Name, c.Email, c.Phone, c.Website, c.TaxNumber,
                c.Address, c.City, c.Country, c.IsActive, c.CreatedAt,
                ProductCount = c.Products.Count,
                InvoiceCount = c.Invoices.Count,
            })
            .ToListAsync());

    [HttpGet("companies/{id:int}")]
    public async Task<IActionResult> GetCompany(int id)
    {
        var c = await db.Companies
            .Include(c => c.Client)
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id, c.ClientId,
                ClientName   = c.Client != null ? c.Client.Name : null,
                c.Name, c.Email, c.Phone, c.Website, c.TaxNumber,
                c.Address, c.City, c.Country, c.IsActive, c.CreatedAt,
                ProductCount = c.Products.Count,
                InvoiceCount = c.Invoices.Count,
            })
            .FirstOrDefaultAsync();
        return c is null ? NotFound() : Ok(c);
    }

    [HttpPost("companies")]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyRequest req)
    {
        var company = new Company
        {
            ClientId  = req.ClientId,
            Name      = req.Name,
            Email     = string.IsNullOrWhiteSpace(req.Email)     ? null : req.Email,
            Phone     = string.IsNullOrWhiteSpace(req.Phone)     ? null : req.Phone,
            Website   = string.IsNullOrWhiteSpace(req.Website)   ? null : req.Website,
            TaxNumber = string.IsNullOrWhiteSpace(req.TaxNumber) ? null : req.TaxNumber,
            Address   = string.IsNullOrWhiteSpace(req.Address)   ? null : req.Address,
            City      = string.IsNullOrWhiteSpace(req.City)      ? null : req.City,
            Country   = string.IsNullOrWhiteSpace(req.Country)   ? null : req.Country,
            IsActive  = req.IsActive,
        };
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        return Ok(company);
    }

    [HttpPut("companies/{id:int}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyRequest req)
    {
        var company = await db.Companies.FindAsync(id);
        if (company is null) return NotFound();
        company.ClientId  = req.ClientId;
        company.Name      = req.Name;
        company.Email     = string.IsNullOrWhiteSpace(req.Email)     ? null : req.Email;
        company.Phone     = string.IsNullOrWhiteSpace(req.Phone)     ? null : req.Phone;
        company.Website   = string.IsNullOrWhiteSpace(req.Website)   ? null : req.Website;
        company.TaxNumber = string.IsNullOrWhiteSpace(req.TaxNumber) ? null : req.TaxNumber;
        company.Address   = string.IsNullOrWhiteSpace(req.Address)   ? null : req.Address;
        company.City      = string.IsNullOrWhiteSpace(req.City)      ? null : req.City;
        company.Country   = string.IsNullOrWhiteSpace(req.Country)   ? null : req.Country;
        company.IsActive  = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(company);
    }

    [HttpDelete("companies/{id:int}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await db.Companies.FindAsync(id);
        if (company is null) return NotFound();
        db.Companies.Remove(company);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Reviews ────────────────────────────────────────────────────────────

    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews()
        => Ok(await db.Reviews.Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new { r.Id, r.ProductId, ProductName = r.Product.Name, r.UserName, r.Rating, r.Comment, r.CreatedAt })
            .ToListAsync());

    [HttpDelete("reviews/{id:int}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var review = await db.Reviews.FindAsync(id);
        if (review is null) return NotFound();
        db.Reviews.Remove(review);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Audit Logs ─────────────────────────────────────────────────────────

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName,
        [FromQuery] string? entityId,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(a => a.EntityId == entityId);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.ChangedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.EntityName,
                a.Action,
                a.EntityId,
                a.OldValues,
                a.NewValues,
                a.ChangedByUserId,
                a.ChangedAt
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, logs });
    }
}

// ── Request models ─────────────────────────────────────────────────────────────
public record CreateUserRequest(string Email, string FullName, string Password, string? Role, int? ClientId);
public record UpdateUserRequest(string FullName, string? Role, int? ClientId);
public record ResetPasswordRequest(string NewPassword);
public record ClientRequest(string Name, string Slug, bool HasECommerce, bool HasCrm, bool HasKanban, bool HasCalendar, bool HasChat, bool IsActive, bool AllowCompanyRewardOverride = true);
public record CategoryRequest(string Name, string Slug, string? Description, bool IsActive);
public record ManufacturerRequest(string Name, string? Website, bool IsActive);
public record UnitRequest(string Name, string Abbreviation, bool IsActive);
public record CompanyRequest(int ClientId, string Name, string? Email, string? Phone, string? Website, string? TaxNumber, string? Address, string? City, string? Country, bool IsActive);
