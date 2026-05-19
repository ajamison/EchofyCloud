using System.ComponentModel.DataAnnotations.Schema;
using Echofy.Domain.Enums;

namespace Echofy.Domain.Entities;

public class Invoice
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? AppUserId { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public decimal TaxRate { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByUserId { get; set; }

    public decimal TotalAmount { get; set; }
    public int RewardPointsAwarded { get; set; }
    public decimal RewardGiftCardAmount { get; set; }
    public string? RewardGiftCardCode { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = [];
    public ThankYouNote? ThankYouNote { get; set; }

    [NotMapped] public decimal SubTotal => TotalAmount;
    [NotMapped] public decimal TaxAmount => 0;
    [NotMapped] public decimal Total => TotalAmount;
}
