using Echofy.Domain.Enums;

namespace Echofy.Application.DTOs;

public class DealDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LeadId { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public DealStage Stage { get; set; }
    public decimal Value { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
