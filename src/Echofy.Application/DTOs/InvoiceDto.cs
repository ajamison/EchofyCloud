namespace Echofy.Application.DTOs;

public class InvoiceListItemDto
{
    public int Id { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Total { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? PaidAt { get; set; }
}

public class ThankYouNoteDto
{
    public DateTime SentAt { get; set; }
    public bool ReferralIncluded { get; set; }
    public string? ReferralCode { get; set; }
}

public class InvoiceDto : InvoiceListItemDto
{
    public string? CustomerPhone { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RewardPointsAwarded { get; set; }
    public decimal RewardGiftCardAmount { get; set; }
    public string? RewardGiftCardCode { get; set; }
    public ThankYouNoteDto? ThankYouNote { get; set; }
}
