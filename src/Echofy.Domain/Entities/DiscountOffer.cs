using Echofy.Domain.Enums;

namespace Echofy.Domain.Entities;

public class DiscountOffer
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }   // % or fixed amount
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;
}
