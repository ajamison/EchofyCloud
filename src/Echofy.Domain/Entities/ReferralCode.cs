namespace Echofy.Domain.Entities;

public class ReferralCode
{
    public int Id { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ReferralUse> Uses { get; set; } = [];
}
