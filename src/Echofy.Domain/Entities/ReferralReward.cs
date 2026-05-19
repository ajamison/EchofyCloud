using Echofy.Domain.Enums;

namespace Echofy.Domain.Entities;

public class ReferralReward
{
    public int Id { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public int ReferralUseId { get; set; }
    public int PointsEarned { get; set; }
    public ReferralRewardStatus Status { get; set; } = ReferralRewardStatus.Pending;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? IssuedAt { get; set; }

    public ReferralUse ReferralUse { get; set; } = null!;
}
