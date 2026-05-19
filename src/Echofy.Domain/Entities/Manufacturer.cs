namespace Echofy.Domain.Entities;

public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<ManufacturerProduct> ManufacturerProducts { get; set; } = [];
}
