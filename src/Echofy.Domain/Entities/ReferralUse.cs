namespace Echofy.Domain.Entities;

public class ReferralUse
{
    public int Id { get; set; }
    public int ReferralCodeId { get; set; }
    public string UsedByUserId { get; set; } = string.Empty;
    public string UsedByEmail { get; set; } = string.Empty;
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;

    public string? WelcomeCouponCode { get; set; }

    public ReferralCode ReferralCode { get; set; } = null!;
    public ReferralReward? Reward { get; set; }
}
