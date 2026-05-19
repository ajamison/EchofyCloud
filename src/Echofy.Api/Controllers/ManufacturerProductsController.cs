using Echofy.Domain.Entities;
using Echofy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/manufacturer-products")]
[Authorize]
public class ManufacturerProductsController(ApplicationDbContext db) : ControllerBase
{
    // ── CRUD ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? manufacturerId, CancellationToken ct)
    {
        var query = db.ManufacturerProducts
            .Include(mp => mp.Manufacturer)
            .Include(mp => mp.UnitOfMeasure)
            .Include(mp => mp.Images)
            .AsQueryable();

        if (manufacturerId.HasValue)
            query = query.Where(mp => mp.ManufacturerId == manufacturerId.Value);

        var result = await query.OrderBy(mp => mp.Manufacturer.Name)
            .Select(mp => new
            {
                mp.Id,
                mp.ManufacturerId,
                ManufacturerName = mp.Manufacturer.Name,
                mp.Name,
                mp.ManufacturerPartNumber,
                mp.Sku,
                mp.Description,
                mp.Size,
                mp.Msrp,
                mp.UnitOfMeasureId,
                UnitOfMeasureName = mp.UnitOfMeasure != null ? mp.UnitOfMeasure.Name : null,
                UnitOfMeasureAbbreviation = mp.UnitOfMeasure != null ? mp.UnitOfMeasure.Abbreviation : null,
                mp.IsActive,
                mp.CreatedAt,
                ProductCount = mp.Products.Count,
                MainImageFileName = mp.Images.Where(i => i.IsMain).Select(i => i.FileName).FirstOrDefault()
                    ?? mp.Images.OrderBy(i => i.DisplayOrder).Select(i => i.FileName).FirstOrDefault()
            })
            .ToListAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var mp = await db.ManufacturerProducts
            .Include(x => x.Manufacturer)
            .Include(x => x.UnitOfMeasure)
            .Include(x => x.Images.OrderBy(i => i.DisplayOrder))
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (mp is null) return NotFound();

        return Ok(new
        {
            mp.Id,
            mp.ManufacturerId,
            ManufacturerName = mp.Manufacturer.Name,
            mp.Name,
            mp.ManufacturerPartNumber,
            mp.Sku,
            mp.Description,
            mp.Size,
            mp.Msrp,
            mp.UnitOfMeasureId,
            UnitOfMeasureName = mp.UnitOfMeasure?.Name,
            UnitOfMeasureAbbreviation = mp.UnitOfMeasure?.Abbreviation,
            mp.IsActive,
            mp.CreatedAt,
            Images = mp.Images.Select(i => new
            {
                i.Id,
                i.ManufacturerProductId,
                i.FileName,
                i.AltText,
                i.IsMain,
                i.DisplayOrder,
                i.UploadedAt,
                Url = $"/uploads/manufacturer-products/{i.FileName}"
            }),
            ProductCount = mp.Products.Count
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] ManufacturerProductRequest req, CancellationToken ct)
    {
        var mp = new ManufacturerProduct
        {
            ManufacturerId = req.ManufacturerId,
            Name = req.Name,
            ManufacturerPartNumber = req.ManufacturerPartNumber,
            Sku = req.Sku,
            Description = req.Description,
            Size = req.Size,
            Msrp = req.Msrp,
            UnitOfMeasureId = req.UnitOfMeasureId,
            IsActive = req.IsActive
        };
        db.ManufacturerProducts.Add(mp);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = mp.Id }, new { mp.Id });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] ManufacturerProductRequest req, CancellationToken ct)
    {
        var mp = await db.ManufacturerProducts.FindAsync([id], ct);
        if (mp is null) return NotFound();

        mp.ManufacturerId = req.ManufacturerId;
        mp.Name = req.Name;
        mp.ManufacturerPartNumber = req.ManufacturerPartNumber;
        mp.Sku = req.Sku;
        mp.Description = req.Description;
        mp.Size = req.Size;
        mp.Msrp = req.Msrp;
        mp.UnitOfMeasureId = req.UnitOfMeasureId;
        mp.IsActive = req.IsActive;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var mp = await db.ManufacturerProducts.FindAsync([id], ct);
        if (mp is null) return NotFound();

        db.ManufacturerProducts.Remove(mp);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Images ─────────────────────────────────────────────────────────────

    [HttpPost("{id:int}/images")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddImage(int id, IFormFile file, [FromForm] string? altText, CancellationToken ct)
    {
        var mp = await db.ManufacturerProducts.FindAsync([id], ct);
        if (mp is null) return NotFound();

        var existingCount = await db.ManufacturerProductImages.CountAsync(i => i.ManufacturerProductId == id, ct);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "manufacturer-products");
        Directory.CreateDirectory(uploadsPath);
        await using var stream = System.IO.File.Create(Path.Combine(uploadsPath, fileName));
        await file.CopyToAsync(stream, ct);

        var image = new ManufacturerProductImage
        {
            ManufacturerProductId = id,
            FileName = fileName,
            AltText = altText,
            IsMain = existingCount == 0,
            DisplayOrder = existingCount
        };

        db.ManufacturerProductImages.Add(image);
        await db.SaveChangesAsync(ct);

        return Ok(new
        {
            image.Id,
            image.ManufacturerProductId,
            image.FileName,
            image.AltText,
            image.IsMain,
            image.DisplayOrder,
            image.UploadedAt,
            Url = $"/uploads/manufacturer-products/{image.FileName}"
        });
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken ct)
    {
        var image = await db.ManufacturerProductImages
            .FirstOrDefaultAsync(i => i.ManufacturerProductId == id && i.Id == imageId, ct);
        if (image is null) return NotFound();

        var wasMain = image.IsMain;
        db.ManufacturerProductImages.Remove(image);
        await db.SaveChangesAsync(ct);

        if (wasMain)
        {
            var next = await db.ManufacturerProductImages
                .Where(i => i.ManufacturerProductId == id)
                .OrderBy(i => i.DisplayOrder)
                .FirstOrDefaultAsync(ct);
            if (next is not null)
            {
                next.IsMain = true;
                await db.SaveChangesAsync(ct);
            }
        }

        return NoContent();
    }

    [HttpPut("{id:int}/images/{imageId:int}/main")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SetMainImage(int id, int imageId, CancellationToken ct)
    {
        var images = await db.ManufacturerProductImages
            .Where(i => i.ManufacturerProductId == id)
            .ToListAsync(ct);

        if (!images.Any(i => i.Id == imageId)) return NotFound();

        foreach (var img in images)
            img.IsMain = img.Id == imageId;

        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record ManufacturerProductRequest(
    int ManufacturerId,
    string Name,
    string? ManufacturerPartNumber,
    string? Sku,
    string? Description,
    string? Size,
    decimal? Msrp,
    int? UnitOfMeasureId,
    bool IsActive);
