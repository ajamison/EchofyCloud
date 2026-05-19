using Echofy.Domain.Enums;

namespace Echofy.Application.DTOs;

public class DiscountOfferDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public bool IsCurrentlyRunning =>
        IsActive && StartDate <= DateTime.UtcNow && EndDate >= DateTime.UtcNow;
}
