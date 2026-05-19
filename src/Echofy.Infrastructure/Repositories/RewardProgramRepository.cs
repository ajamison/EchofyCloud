using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class RewardProgramRepository(ApplicationDbContext db) : IRewardProgramRepository
{
    public Task<List<RewardProgram>> GetAllForClientAsync(int clientId, CancellationToken ct = default)
        => db.RewardPrograms
            .Include(p => p.Company)
            .Include(p => p.Tiers.OrderBy(t => t.DisplayOrder))
            .Where(p => p.ClientId == clientId)
            .OrderBy(p => p.CompanyId)
            .ThenBy(p => p.Name)
            .ToListAsync(ct);

    public Task<RewardProgram?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.RewardPrograms
            .Include(p => p.Company)
            .Include(p => p.Tiers.OrderBy(t => t.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<RewardProgram?> GetEffectiveProgramAsync(int clientId, int? companyId, CancellationToken ct = default)
    {
        if (companyId.HasValue)
        {
            var client = await db.Clients.FindAsync([clientId], ct);
            if (client?.AllowCompanyRewardOverride == true)
            {
                var companyProgram = await db.RewardPrograms
                    .Include(p => p.Tiers.Where(t => t.IsActive))
                    .FirstOrDefaultAsync(p => p.CompanyId == companyId.Value && p.IsActive, ct);
                if (companyProgram is not null) return companyProgram;
            }
        }

        return await db.RewardPrograms
            .Include(p => p.Tiers.Where(t => t.IsActive))
            .FirstOrDefaultAsync(p => p.ClientId == clientId && p.CompanyId == null && p.IsActive, ct);
    }

    public Task<RewardTier?> GetTierByIdAsync(int tierId, CancellationToken ct = default)
        => db.RewardTiers.FindAsync([tierId], ct).AsTask();

    public async Task AddAsync(RewardProgram program, CancellationToken ct = default)
        => await db.RewardPrograms.AddAsync(program, ct);

    public async Task AddTierAsync(RewardTier tier, CancellationToken ct = default)
        => await db.RewardTiers.AddAsync(tier, ct);

    public void Update(RewardProgram program) => db.RewardPrograms.Update(program);
    public void UpdateTier(RewardTier tier) => db.RewardTiers.Update(tier);
    public void Remove(RewardProgram program) => db.RewardPrograms.Remove(program);
    public void RemoveTier(RewardTier tier) => db.RewardTiers.Remove(tier);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
