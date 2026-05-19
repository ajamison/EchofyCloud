using Echofy.Domain.Enums;

namespace Echofy.Domain.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public CouponType CouponType { get; set; }
    public decimal Value { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
}
