namespace Echofy.Domain.Entities;

public class ProductShortId
{
    public int Id { get; set; }
    public int? ProductId { get; set; }                    // null = unassigned (pre-printed label)
    public string Code { get; set; } = string.Empty;      // 8-char slug, globally unique
    public string? Label { get; set; }                    // e.g. "Shelf Label", "Point of Sale"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AssignedAt { get; set; }              // set when product is assigned

    public Product? Product { get; set; }
}
