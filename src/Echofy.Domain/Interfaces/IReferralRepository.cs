using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IReferralRepository
{
    Task<ReferralCode?> GetCodeByUserIdAsync(string userId, CancellationToken ct = default);
    Task<ReferralCode?> GetCodeByValueAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<ReferralUse>> GetUsesForCodeAsync(int codeId, CancellationToken ct = default);
    Task<IReadOnlyList<ReferralReward>> GetRewardsForUserAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<ReferralReward>> GetAllRewardsAsync(CancellationToken ct = default);
    Task AddCodeAsync(ReferralCode code, CancellationToken ct = default);
    Task AddUseAsync(ReferralUse use, CancellationToken ct = default);
    Task AddRewardAsync(ReferralReward reward, CancellationToken ct = default);
    Task AddCouponAsync(Coupon coupon, CancellationToken ct = default);
    Task<bool> CouponCodeExistsAsync(string code, CancellationToken ct = default);
    Task<ReferralReward?> GetRewardByIdAsync(int id, CancellationToken ct = default);
    void UpdateReward(ReferralReward reward);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
