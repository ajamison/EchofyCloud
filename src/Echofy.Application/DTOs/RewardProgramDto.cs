namespace Echofy.Application.DTOs;

public class RewardTierDto
{
    public int Id { get; set; }
    public int RewardProgramId { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal MinInvoiceAmount { get; set; }
    public int PointsForReferrer { get; set; }
    public decimal GiftCardAmount { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class RewardProgramDto
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RewardTierDto> Tiers { get; set; } = [];
}
