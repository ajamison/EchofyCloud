namespace Echofy.Domain.Entities;

public class RewardTier
{
    public int Id { get; set; }
    public int RewardProgramId { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal MinInvoiceAmount { get; set; }
    public int PointsForReferrer { get; set; }
    public decimal GiftCardAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public RewardProgram RewardProgram { get; set; } = null!;
}
