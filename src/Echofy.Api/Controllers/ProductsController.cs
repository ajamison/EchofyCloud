using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController(IProductService productService) : ControllerBase
{
    private int? ClientId => int.TryParse(User.FindFirstValue("clientId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] bool? activeOnly, CancellationToken ct)
        => Ok(await productService.GetAllAsync(ClientId, search, activeOnly, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var product = await productService.GetWithDetailsAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("short/{shortId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByShortId(string shortId, CancellationToken ct)
    {
        var product = await productService.GetByShortIdAsync(shortId, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("barcode/{upc}")]
    public async Task<IActionResult> GetByUpc(string upc, CancellationToken ct)
    {
        var product = await productService.GetByUpcAsync(upc, ct);
        return product is null ? NotFound(new { message = $"No product found for UPC '{upc}'." }) : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] ProductDto dto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var created = await productService.CreateAsync(dto, changedByUserId: userId, ct: ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var updated = await productService.UpdateAsync(id, dto, clientId: ClientId, changedByUserId: userId, ct: ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await productService.DeleteAsync(id, ClientId, ct);
        return deleted ? NoContent() : NotFound();
    }

    // ── Images ─────────────────────────────────────────────────────────────

    [HttpPost("{id:int}/images")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddImage(int id, IFormFile file, [FromForm] string? altText, [FromForm] string? sku, CancellationToken ct)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
        Directory.CreateDirectory(uploadsPath);
        await using var stream = System.IO.File.Create(Path.Combine(uploadsPath, fileName));
        await file.CopyToAsync(stream, ct);

        var image = await productService.AddImageAsync(id, fileName, altText, sku, ct);
        return Ok(image);
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken ct)
    {
        var deleted = await productService.DeleteImageAsync(id, imageId, ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("{id:int}/images/{imageId:int}/main")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SetMainImage(int id, int imageId, CancellationToken ct)
    {
        var result = await productService.SetMainImageAsync(id, imageId, ct);
        return result ? NoContent() : NotFound();
    }

    // ── Discount Offers ────────────────────────────────────────────────────

    [HttpGet("{id:int}/offers")]
    public async Task<IActionResult> GetOffers(int id, CancellationToken ct)
        => Ok(await productService.GetDiscountOffersAsync(id, ct));

    [HttpPost("{id:int}/offers")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateOffer(int id, [FromBody] DiscountOfferDto dto, CancellationToken ct)
        => Ok(await productService.CreateDiscountOfferAsync(id, dto, ct));

    [HttpPut("{id:int}/offers/{offerId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateOffer(int id, int offerId, [FromBody] DiscountOfferDto dto, CancellationToken ct)
    {
        var updated = await productService.UpdateDiscountOfferAsync(id, offerId, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}/offers/{offerId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteOffer(int id, int offerId, CancellationToken ct)
    {
        var deleted = await productService.DeleteDiscountOfferAsync(id, offerId, ct);
        return deleted ? NoContent() : NotFound();
    }

    // ── Short IDs ──────────────────────────────────────────────────────────

    [HttpPost("{id:int}/short-ids")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GenerateShortId(int id, [FromBody] GenerateShortIdRequest request, CancellationToken ct)
        => Ok(await productService.GenerateAdditionalShortIdAsync(id, request.Label, ct));

    [HttpDelete("{id:int}/short-ids/{shortIdId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteShortId(int id, int shortIdId, CancellationToken ct)
    {
        var deleted = await productService.DeleteAdditionalShortIdAsync(id, shortIdId, ct);
        return deleted ? NoContent() : NotFound();
    }
}

public record GenerateShortIdRequest(string? Label);
