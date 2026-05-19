using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IRewardProgramRepository
{
    Task<List<RewardProgram>> GetAllForClientAsync(int clientId, CancellationToken ct = default);
    Task<RewardProgram?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RewardProgram?> GetEffectiveProgramAsync(int clientId, int? companyId, CancellationToken ct = default);
    Task<RewardTier?> GetTierByIdAsync(int tierId, CancellationToken ct = default);
    Task AddAsync(RewardProgram program, CancellationToken ct = default);
    Task AddTierAsync(RewardTier tier, CancellationToken ct = default);
    void Update(RewardProgram program);
    void UpdateTier(RewardTier tier);
    void Remove(RewardProgram program);
    void RemoveTier(RewardTier tier);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
