namespace Echofy.Domain.Entities;

public class ProductPriceHistory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }   // null = current active price
    public string? ChangedByUserId { get; set; }

    public Product Product { get; set; } = null!;
}
