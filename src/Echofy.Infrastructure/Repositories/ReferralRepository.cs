using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class ReferralRepository(ApplicationDbContext db) : IReferralRepository
{
    public async Task<ReferralCode?> GetCodeByUserIdAsync(string userId, CancellationToken ct = default)
        => await db.ReferralCodes.FirstOrDefaultAsync(c => c.AppUserId == userId, ct);

    public async Task<ReferralCode?> GetCodeByValueAsync(string code, CancellationToken ct = default)
        => await db.ReferralCodes.FirstOrDefaultAsync(c => c.Code == code, ct);

    public async Task<IReadOnlyList<ReferralUse>> GetUsesForCodeAsync(int codeId, CancellationToken ct = default)
        => await db.ReferralUses
            .Include(u => u.Reward)
            .Where(u => u.ReferralCodeId == codeId)
            .OrderByDescending(u => u.UsedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ReferralReward>> GetRewardsForUserAsync(string userId, CancellationToken ct = default)
        => await db.ReferralRewards
            .Include(r => r.ReferralUse)
            .Where(r => r.AppUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ReferralReward>> GetAllRewardsAsync(CancellationToken ct = default)
        => await db.ReferralRewards
            .Include(r => r.ReferralUse)
                .ThenInclude(u => u.ReferralCode)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task<ReferralReward?> GetRewardByIdAsync(int id, CancellationToken ct = default)
        => await db.ReferralRewards.FindAsync([id], ct);

    public async Task AddCodeAsync(ReferralCode code, CancellationToken ct = default)
        => await db.ReferralCodes.AddAsync(code, ct);

    public async Task AddUseAsync(ReferralUse use, CancellationToken ct = default)
        => await db.ReferralUses.AddAsync(use, ct);

    public async Task AddRewardAsync(ReferralReward reward, CancellationToken ct = default)
        => await db.ReferralRewards.AddAsync(reward, ct);

    public async Task AddCouponAsync(Coupon coupon, CancellationToken ct = default)
        => await db.Coupons.AddAsync(coupon, ct);

    public async Task<bool> CouponCodeExistsAsync(string code, CancellationToken ct = default)
        => await db.Coupons.AnyAsync(c => c.Code == code, ct);

    public void UpdateReward(ReferralReward reward)
        => db.ReferralRewards.Update(reward);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
