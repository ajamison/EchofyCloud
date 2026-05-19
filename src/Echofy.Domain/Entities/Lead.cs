using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;

namespace Echofy.Domain.Entities;

public class Lead : IAuditable
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public decimal EstimatedValue { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ICollection<Deal> Deals { get; set; } = [];
}
