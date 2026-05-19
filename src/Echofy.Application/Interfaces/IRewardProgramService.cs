using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IRewardProgramService
{
    Task<List<RewardProgramDto>> GetAllForClientAsync(int clientId, CancellationToken ct = default);
    Task<RewardProgramDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<RewardProgramDto> CreateAsync(CreateRewardProgramRequest req, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, UpdateRewardProgramRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<RewardTierDto?> AddTierAsync(int programId, SaveRewardTierRequest req, CancellationToken ct = default);
    Task<bool> UpdateTierAsync(int tierId, SaveRewardTierRequest req, CancellationToken ct = default);
    Task<bool> DeleteTierAsync(int tierId, CancellationToken ct = default);
    Task<ApplyRewardResult?> ApplyToInvoiceAsync(int invoiceId, CancellationToken ct = default);
}

public record CreateRewardProgramRequest(int ClientId, int? CompanyId, string Name, bool IsActive);
public record UpdateRewardProgramRequest(string Name, bool IsActive);
public record SaveRewardTierRequest(string Label, decimal MinInvoiceAmount, int PointsForReferrer, decimal GiftCardAmount, bool IsActive, int DisplayOrder);
public record ApplyRewardResult(int PointsAwarded, decimal GiftCardAmount, string? GiftCardCode);
