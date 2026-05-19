namespace Echofy.Domain.Entities;

public class UnitOfMeasure
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;         // e.g. "Ounce"
    public string Abbreviation { get; set; } = string.Empty; // e.g. "oz"
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Products { get; set; } = [];
}
