using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;

namespace Echofy.Domain.Entities;

public class Deal : IAuditable
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LeadId { get; set; }
    public DealStage Stage { get; set; } = DealStage.Prospecting;
    public decimal Value { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }

    public Lead Lead { get; set; } = null!;
}
