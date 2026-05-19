using System.Security.Cryptography;
using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class ReferralService(IReferralRepository repo) : IReferralService
{
    private const int    ReferrerPoints          = 100;
    private const decimal WelcomeDiscountAmount  = 5.00m;

    public async Task<ReferralDto> GetOrCreateReferralAsync(string userId, string baseUrl, CancellationToken ct = default)
    {
        var code = await repo.GetCodeByUserIdAsync(userId, ct);
        if (code is null)
        {
            code = new ReferralCode
            {
                AppUserId = userId,
                Code = GenerateCode(8),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await repo.AddCodeAsync(code, ct);
            await repo.SaveChangesAsync(ct);
        }

        var uses    = await repo.GetUsesForCodeAsync(code.Id, ct);
        var rewards = await repo.GetRewardsForUserAsync(userId, ct);

        var shareUrl  = $"{baseUrl.TrimEnd('/')}/Account/Register?ref={Uri.EscapeDataString(code.Code)}";
        var shareText = $"Hey! I've been using Echofy and wanted to invite you. " +
                        $"Sign up with my referral code {code.Code} and you'll get ${WelcomeDiscountAmount:N0} off your first order. " +
                        $"Sign up here: {shareUrl}";

        return new ReferralDto
        {
            CodeId         = code.Id,
            Code           = code.Code,
            ShareUrl       = shareUrl,
            ShareText      = shareText,
            TotalReferrals = uses.Count,
            TotalPoints    = rewards.Where(r => r.Status == ReferralRewardStatus.Issued).Sum(r => r.PointsEarned),
            PendingPoints  = rewards.Where(r => r.Status is ReferralRewardStatus.Pending or ReferralRewardStatus.Approved).Sum(r => r.PointsEarned),
            RecentUses     = uses.OrderByDescending(u => u.UsedAt).Take(10).Select(u => new ReferralUseDto
            {
                UsedByEmail       = MaskEmail(u.UsedByEmail),
                UsedAt            = u.UsedAt,
                HasReward         = u.Reward is not null,
                RewardStatus      = u.Reward?.Status.ToString() ?? string.Empty,
                WelcomeCouponCode = u.WelcomeCouponCode
            }).ToList(),
            Rewards = rewards.OrderByDescending(r => r.CreatedAt).Select(r => new ReferralRewardDto
            {
                Id           = r.Id,
                PointsEarned = r.PointsEarned,
                Status       = r.Status.ToString(),
                Description  = r.Description,
                CreatedAt    = r.CreatedAt,
                IssuedAt     = r.IssuedAt,
            }).ToList()
        };
    }

    public async Task<string?> UseReferralCodeAsync(string code, string newUserId, string newUserEmail, CancellationToken ct = default)
    {
        var referralCode = await repo.GetCodeByValueAsync(code, ct);
        if (referralCode is null || !referralCode.IsActive)
            return null;

        if (referralCode.AppUserId == newUserId)
            return null;

        var couponCode = await GenerateUniqueCouponCodeAsync("WELCOME-", ct);

        var use = new ReferralUse
        {
            ReferralCodeId    = referralCode.Id,
            UsedByUserId      = newUserId,
            UsedByEmail       = newUserEmail,
            UsedAt            = DateTime.UtcNow,
            WelcomeCouponCode = couponCode
        };
        await repo.AddUseAsync(use, ct);
        await repo.SaveChangesAsync(ct);

        var coupon = new Coupon
        {
            Code       = couponCode,
            CouponType = CouponType.FixedCart,
            Value      = WelcomeDiscountAmount,
            IsActive   = true,
            ExpiresAt  = DateTime.UtcNow.AddDays(90)
        };
        await repo.AddCouponAsync(coupon, ct);

        var reward = new ReferralReward
        {
            AppUserId     = referralCode.AppUserId,
            ReferralUseId = use.Id,
            PointsEarned  = ReferrerPoints,
            Status        = ReferralRewardStatus.Pending,
            Description   = $"Referral reward for inviting {MaskEmail(newUserEmail)}",
            CreatedAt     = DateTime.UtcNow
        };
        await repo.AddRewardAsync(reward, ct);
        await repo.SaveChangesAsync(ct);

        return couponCode;
    }

    public async Task<IReadOnlyList<AdminReferralDto>> GetAllForAdminAsync(CancellationToken ct = default)
    {
        var rewards = await repo.GetAllRewardsAsync(ct);
        return rewards.OrderByDescending(r => r.CreatedAt).Select(r => new AdminReferralDto
        {
            RewardId      = r.Id,
            ReferrerId    = r.AppUserId,
            ReferralCode  = r.ReferralUse.ReferralCode.Code,
            UsedByEmail   = r.ReferralUse.UsedByEmail,
            WelcomeCoupon = r.ReferralUse.WelcomeCouponCode,
            PointsEarned  = r.PointsEarned,
            Status        = r.Status.ToString(),
            UsedAt        = r.ReferralUse.UsedAt,
            ApprovedAt    = r.ApprovedAt,
            IssuedAt      = r.IssuedAt,
        }).ToList();
    }

    public async Task<bool> ApproveRewardAsync(int rewardId, CancellationToken ct = default)
    {
        var reward = await repo.GetRewardByIdAsync(rewardId, ct);
        if (reward is null || reward.Status != ReferralRewardStatus.Pending) return false;

        reward.Status     = ReferralRewardStatus.Approved;
        reward.ApprovedAt = DateTime.UtcNow;
        repo.UpdateReward(reward);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> IssueRewardAsync(int rewardId, CancellationToken ct = default)
    {
        var reward = await repo.GetRewardByIdAsync(rewardId, ct);
        if (reward is null || reward.Status != ReferralRewardStatus.Approved) return false;

        reward.Status   = ReferralRewardStatus.Issued;
        reward.IssuedAt = DateTime.UtcNow;
        repo.UpdateReward(reward);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CancelRewardAsync(int rewardId, CancellationToken ct = default)
    {
        var reward = await repo.GetRewardByIdAsync(rewardId, ct);
        if (reward is null || reward.Status == ReferralRewardStatus.Issued) return false;

        reward.Status = ReferralRewardStatus.Cancelled;
        repo.UpdateReward(reward);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    private async Task<string> GenerateUniqueCouponCodeAsync(string prefix, CancellationToken ct)
    {
        string code;
        do { code = prefix + GenerateCode(6); }
        while (await repo.CouponCodeExistsAsync(code, ct));
        return code;
    }

    private static string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(RandomNumberGenerator.GetItems<char>(chars, length));
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return email;
        return email[0] + new string('*', Math.Min(at - 1, 4)) + email[at..];
    }
}
