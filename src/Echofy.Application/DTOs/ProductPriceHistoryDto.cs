namespace Echofy.Application.DTOs;

public class ProductPriceHistoryDto
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsCurrent => EffectiveTo is null;
}
