using Echofy.Domain.Enums;

namespace Echofy.Application.DTOs;

public class LeadDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public LeadStatus Status { get; set; }
    public decimal EstimatedValue { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DealCount { get; set; }
}
