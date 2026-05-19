namespace Echofy.Application.DTOs;

public class ReferralDto
{
    public int CodeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ShareUrl { get; set; } = string.Empty;
    public string ShareText { get; set; } = string.Empty;
    public int TotalReferrals { get; set; }
    public int TotalPoints { get; set; }
    public int PendingPoints { get; set; }
    public IReadOnlyList<ReferralUseDto> RecentUses { get; set; } = [];
    public IReadOnlyList<ReferralRewardDto> Rewards { get; set; } = [];
}

public class ReferralUseDto
{
    public string UsedByEmail { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; }
    public bool HasReward { get; set; }
    public string RewardStatus { get; set; } = string.Empty;
    public string? WelcomeCouponCode { get; set; }
}

public class ReferralRewardDto
{
    public int Id { get; set; }
    public int PointsEarned { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
}

public class AdminReferralDto
{
    public int RewardId { get; set; }
    public string ReferrerId { get; set; } = string.Empty;
    public string ReferrerEmail { get; set; } = string.Empty;
    public string ReferrerName { get; set; } = string.Empty;
    public string ReferralCode { get; set; } = string.Empty;
    public string UsedByEmail { get; set; } = string.Empty;
    public string? WelcomeCoupon { get; set; }
    public int PointsEarned { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
}
