using Echofy.Domain.Interfaces;

namespace Echofy.Domain.Entities;

public class ManufacturerProduct : IAuditable
{
    public int Id { get; set; }
    public int ManufacturerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ManufacturerPartNumber { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public string? Size { get; set; }
    public decimal? Msrp { get; set; }
    public int? UnitOfMeasureId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }

    public Manufacturer Manufacturer { get; set; } = null!;
    public UnitOfMeasure? UnitOfMeasure { get; set; }
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<ManufacturerProductImage> Images { get; set; } = [];
}
