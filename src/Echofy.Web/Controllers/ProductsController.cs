using System.Security.Claims;
using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Enums;
using Echofy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Echofy.Web.Controllers;

[Authorize]
public class ProductsController(
    IProductService productService,
    IFavoriteService favoriteService,
    IRecommendationService recommendationService,
    ApplicationDbContext db,
    IWebHostEnvironment env) : Controller
{
    // ── List ───────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(string? search)
    {
        ViewData["ActivePage"] = "Products";
        ViewData["Search"] = search;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var products = await productService.GetAllAsync(search: search);

        if (userId is not null)
        {
            var clientId = GetClientId();
            ViewBag.Recommendations = await recommendationService.GetForUserAsync(userId, 6, clientId);
        }

        return View(products);
    }

    // ── Details ────────────────────────────────────────────────────────────

    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Products";
        var product = await productService.GetWithDetailsAsync(id);
        if (product is null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
            ViewBag.IsFavorited = await favoriteService.IsFavoritedAsync(userId, product.Id);

        return View(product);
    }

    // ── Create ─────────────────────────────────────────────────────────────

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["ActivePage"] = "Products";
        await PopulateCompaniesAsync();
        await PopulateCategoriesAsync();
        await PopulateManufacturersAsync();
        await PopulateUnitsOfMeasureAsync();
        return View(new ProductDto { IsActive = true });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCompaniesAsync();
            await PopulateCategoriesAsync();
            await PopulateManufacturersAsync(dto.ManufacturerId);
            await PopulateUnitsOfMeasureAsync(dto.UnitOfMeasureId);
            return View(dto);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await productService.CreateAsync(dto, changedByUserId: userId);
        return RedirectToAction(nameof(Index));
    }

    // ── Edit ───────────────────────────────────────────────────────────────

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActivePage"] = "Products";
        var product = await productService.GetWithDetailsAsync(id);
        if (product is null) return NotFound();
        await PopulateCompaniesAsync();
        await PopulateCategoriesAsync();
        await PopulateManufacturersAsync(product.ManufacturerId);
        await PopulateUnitsOfMeasureAsync(product.UnitOfMeasureId);
        await PopulateAvailableImagesAsync(id, product.Images.Select(i => i.FileName).ToHashSet());
        return View(product);
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            // Re-load details for the history/offers/images/shortids sections
            var full = await productService.GetWithDetailsAsync(id);
            dto.PriceHistory = full?.PriceHistory ?? [];
            dto.DiscountOffers = full?.DiscountOffers ?? [];
            dto.Images = full?.Images ?? [];
            dto.AdditionalShortIds = full?.AdditionalShortIds ?? [];
            dto.ShortId = full?.ShortId ?? string.Empty;
            await PopulateCompaniesAsync();
            await PopulateCategoriesAsync();
            await PopulateManufacturersAsync(dto.ManufacturerId);
            await PopulateUnitsOfMeasureAsync(dto.UnitOfMeasureId);
            return View(dto);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await productService.UpdateAsync(id, dto, changedByUserId: userId);
        return RedirectToAction(nameof(Edit), new { id });
    }

    // ── QR Code ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> QrCode(int id, string? code = null)
    {
        var product = await productService.GetByIdAsync(id);
        if (product is null) return NotFound();

        var slug = string.IsNullOrWhiteSpace(code) ? product.ShortId : code;
        var url = $"{Request.Scheme}://{Request.Host}/p/{slug}";

        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var svgCode = new SvgQRCode(qrData);
        var svg = svgCode.GetGraphic(5);

        return Content(svg, "image/svg+xml");
    }

    // ── Additional ShortIds ────────────────────────────────────────────────

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateShortId(int id, string? label)
    {
        await productService.GenerateAdditionalShortIdAsync(id, label);
        return RedirectToAction(nameof(Edit), new { id });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteShortId(int id, int shortIdId)
    {
        await productService.DeleteAdditionalShortIdAsync(id, shortIdId);
        return RedirectToAction(nameof(Edit), new { id });
    }

    // ── Image upload ───────────────────────────────────────────────────────

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(int id, IFormFile? file, string? altText, string? sku)
    {
        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Please select an image file.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
        {
            TempData["Error"] = "Unsupported file type. Allowed: jpg, png, webp, gif.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // Save file to shared image library: wwwroot/uploads/images/
        var folder = Path.Combine(env.WebRootPath, "uploads", "images");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
            await file.CopyToAsync(stream);

        await productService.AddImageAsync(id, fileName, altText, sku);
        return RedirectToAction(nameof(Edit), new { id });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignExistingImage(int id, string fileName, string? altText, string? sku)
    {
        await productService.AddImageAsync(id, fileName, altText, sku);
        return RedirectToAction(nameof(Edit), new { id });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var product = await productService.GetWithDetailsAsync(id);
        var image = product?.Images.FirstOrDefault(i => i.Id == imageId);
        if (image is not null)
        {
            // Only delete physical file if no other product references the same image
            var fileInUse = await db.ProductImages
                .AnyAsync(i => i.FileName == image.FileName && i.Id != imageId);
            if (!fileInUse)
            {
                var filePath = Path.Combine(env.WebRootPath, "uploads", "images", image.FileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
        }

        await productService.DeleteImageAsync(id, imageId);
        return RedirectToAction(nameof(Edit), new { id });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetMainImage(int id, int imageId)
    {
        await productService.SetMainImageAsync(id, imageId);
        return RedirectToAction(nameof(Edit), new { id });
    }

    // ── Discount Offers ────────────────────────────────────────────────────

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> CreateOffer(int productId)
    {
        ViewData["ActivePage"] = "Products";
        var product = await productService.GetByIdAsync(productId);
        if (product is null) return NotFound();

        ViewData["ProductName"] = product.Name;
        ViewData["ProductId"] = productId;
        ViewData["FormTitle"] = "Add Discount Offer";
        PopulateDiscountTypes();

        return View("DiscountOfferForm", new DiscountOfferDto
        {
            ProductId = productId,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(7),
            IsActive = true
        });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOffer(int productId, DiscountOfferDto dto)
    {
        if (!ModelState.IsValid)
        {
            var product = await productService.GetByIdAsync(productId);
            ViewData["ProductName"] = product?.Name;
            ViewData["ProductId"] = productId;
            ViewData["FormTitle"] = "Add Discount Offer";
            PopulateDiscountTypes();
            return View("DiscountOfferForm", dto);
        }

        await productService.CreateDiscountOfferAsync(productId, dto);
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> EditOffer(int productId, int offerId)
    {
        ViewData["ActivePage"] = "Products";
        var product = await productService.GetByIdAsync(productId);
        if (product is null) return NotFound();

        var offers = await productService.GetDiscountOffersAsync(productId);
        var offer = offers.FirstOrDefault(o => o.Id == offerId);
        if (offer is null) return NotFound();

        ViewData["ProductName"] = product.Name;
        ViewData["ProductId"] = productId;
        ViewData["FormTitle"] = "Edit Discount Offer";
        PopulateDiscountTypes(offer.DiscountType);

        return View("DiscountOfferForm", offer);
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditOffer(int productId, int offerId, DiscountOfferDto dto)
    {
        if (!ModelState.IsValid)
        {
            var product = await productService.GetByIdAsync(productId);
            ViewData["ProductName"] = product?.Name;
            ViewData["ProductId"] = productId;
            ViewData["FormTitle"] = "Edit Discount Offer";
            PopulateDiscountTypes(dto.DiscountType);
            return View("DiscountOfferForm", dto);
        }

        await productService.UpdateDiscountOfferAsync(productId, offerId, dto);
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOffer(int productId, int offerId)
    {
        await productService.DeleteDiscountOfferAsync(productId, offerId);
        return RedirectToAction(nameof(Edit), new { id = productId });
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task PopulateCategoriesAsync()
    {
        ViewBag.Categories = await db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync();
    }

    private async Task PopulateCompaniesAsync(int? selectedId = null)
    {
        var companies = await db.Companies
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();

        companies.Insert(0, new SelectListItem("— No Company —", ""));
        ViewBag.Companies = new SelectList(companies, "Value", "Text", selectedId?.ToString());
    }

    private async Task PopulateManufacturersAsync(int? selectedId = null)
    {
        var items = await db.Manufacturers
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .Select(m => new SelectListItem(m.Name, m.Id.ToString()))
            .ToListAsync();
        items.Insert(0, new SelectListItem("— None —", ""));
        ViewBag.Manufacturers = new SelectList(items, "Value", "Text", selectedId?.ToString());
    }

    private async Task PopulateUnitsOfMeasureAsync(int? selectedId = null)
    {
        var items = await db.UnitsOfMeasure
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => new SelectListItem($"{u.Name} ({u.Abbreviation})", u.Id.ToString()))
            .ToListAsync();
        items.Insert(0, new SelectListItem("— None —", ""));
        ViewBag.UnitsOfMeasure = new SelectList(items, "Value", "Text", selectedId?.ToString());
    }

    private void PopulateDiscountTypes(DiscountType selected = DiscountType.Percentage)
    {
        ViewBag.DiscountTypes = new List<SelectListItem>
        {
            new("Percentage (%)", DiscountType.Percentage.ToString()) { Selected = selected == DiscountType.Percentage },
            new("Fixed Amount ($)", DiscountType.FixedAmount.ToString()) { Selected = selected == DiscountType.FixedAmount }
        };
    }

    private int? GetClientId()
    {
        var raw = User.FindFirstValue("clientId");
        return int.TryParse(raw, out var id) ? id : null;
    }

    private async Task PopulateAvailableImagesAsync(int productId, HashSet<string> assignedFileNames)
    {
        // Show images from other products that are not already on this product
        var available = await db.ProductImages
            .Where(i => i.ProductId != productId && !assignedFileNames.Contains(i.FileName))
            .Select(i => new { i.FileName, i.AltText })
            .Distinct()
            .ToListAsync();
        ViewBag.AvailableImages = available;
    }
}
