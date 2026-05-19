using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IReferralService
{
    Task<ReferralDto> GetOrCreateReferralAsync(string userId, string baseUrl, CancellationToken ct = default);

    /// <summary>
    /// Records a referral use and creates a welcome discount coupon for the new user.
    /// Returns the generated coupon code, or null if the referral code was invalid.
    /// </summary>
    Task<string?> UseReferralCodeAsync(string code, string newUserId, string newUserEmail, CancellationToken ct = default);

    Task<IReadOnlyList<AdminReferralDto>> GetAllForAdminAsync(CancellationToken ct = default);
    Task<bool> ApproveRewardAsync(int rewardId, CancellationToken ct = default);
    Task<bool> IssueRewardAsync(int rewardId, CancellationToken ct = default);
    Task<bool> CancelRewardAsync(int rewardId, CancellationToken ct = default);
}
