namespace Echofy.Domain.Entities;

public class ThankYouNote
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int? CompanyId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomMessage { get; set; }
    public bool ReferralIncluded { get; set; }
    public string? ReferralCode { get; set; }
    public string? ReferralUrl { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
