using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class RewardProgramService(
    IRewardProgramRepository repo,
    IInvoiceRepository invoiceRepo) : IRewardProgramService
{
    public async Task<List<RewardProgramDto>> GetAllForClientAsync(int clientId, CancellationToken ct = default)
        => (await repo.GetAllForClientAsync(clientId, ct)).Select(ToDto).ToList();

    public async Task<RewardProgramDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var p = await repo.GetByIdAsync(id, ct);
        return p is null ? null : ToDto(p);
    }

    public async Task<RewardProgramDto> CreateAsync(CreateRewardProgramRequest req, CancellationToken ct = default)
    {
        var program = new RewardProgram
        {
            ClientId  = req.ClientId,
            CompanyId = req.CompanyId,
            Name      = req.Name,
            IsActive  = req.IsActive,
            CreatedAt = DateTime.UtcNow,
        };
        await repo.AddAsync(program, ct);
        await repo.SaveChangesAsync(ct);
        return ToDto(program);
    }

    public async Task<bool> UpdateAsync(int id, UpdateRewardProgramRequest req, CancellationToken ct = default)
    {
        var program = await repo.GetByIdAsync(id, ct);
        if (program is null) return false;
        program.Name     = req.Name;
        program.IsActive = req.IsActive;
        repo.Update(program);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var program = await repo.GetByIdAsync(id, ct);
        if (program is null) return false;
        repo.Remove(program);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<RewardTierDto?> AddTierAsync(int programId, SaveRewardTierRequest req, CancellationToken ct = default)
    {
        var program = await repo.GetByIdAsync(programId, ct);
        if (program is null) return null;
        var tier = new RewardTier
        {
            RewardProgramId   = programId,
            Label             = req.Label,
            MinInvoiceAmount  = req.MinInvoiceAmount,
            PointsForReferrer = req.PointsForReferrer,
            GiftCardAmount    = req.GiftCardAmount,
            IsActive          = req.IsActive,
            DisplayOrder      = req.DisplayOrder,
        };
        await repo.AddTierAsync(tier, ct);
        await repo.SaveChangesAsync(ct);
        return ToTierDto(tier);
    }

    public async Task<bool> UpdateTierAsync(int tierId, SaveRewardTierRequest req, CancellationToken ct = default)
    {
        var tier = await repo.GetTierByIdAsync(tierId, ct);
        if (tier is null) return false;
        tier.Label             = req.Label;
        tier.MinInvoiceAmount  = req.MinInvoiceAmount;
        tier.PointsForReferrer = req.PointsForReferrer;
        tier.GiftCardAmount    = req.GiftCardAmount;
        tier.IsActive          = req.IsActive;
        tier.DisplayOrder      = req.DisplayOrder;
        repo.UpdateTier(tier);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteTierAsync(int tierId, CancellationToken ct = default)
    {
        var tier = await repo.GetTierByIdAsync(tierId, ct);
        if (tier is null) return false;
        repo.RemoveTier(tier);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ApplyRewardResult?> ApplyToInvoiceAsync(int invoiceId, CancellationToken ct = default)
    {
        var invoice = await invoiceRepo.GetByIdAsync(invoiceId, null, ct);
        if (invoice is null) return null;

        var clientId = invoice.Company?.ClientId;
        if (!clientId.HasValue) return null;

        var program = await repo.GetEffectiveProgramAsync(clientId.Value, invoice.CompanyId, ct);
        if (program is null) return null;

        var tier = program.Tiers
            .Where(t => t.IsActive && t.MinInvoiceAmount <= invoice.TotalAmount)
            .OrderByDescending(t => t.MinInvoiceAmount)
            .FirstOrDefault();

        if (tier is null) return null;

        var giftCardCode = tier.GiftCardAmount > 0
            ? $"GIFT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}"
            : null;

        invoice.RewardPointsAwarded  = tier.PointsForReferrer;
        invoice.RewardGiftCardAmount = tier.GiftCardAmount;
        invoice.RewardGiftCardCode   = giftCardCode;

        invoiceRepo.Update(invoice);
        await invoiceRepo.SaveChangesAsync(ct);

        return new ApplyRewardResult(tier.PointsForReferrer, tier.GiftCardAmount, giftCardCode);
    }

    private static RewardProgramDto ToDto(RewardProgram p) => new()
    {
        Id          = p.Id,
        ClientId    = p.ClientId,
        CompanyId   = p.CompanyId,
        CompanyName = p.Company?.Name,
        Name        = p.Name,
        IsActive    = p.IsActive,
        CreatedAt   = p.CreatedAt,
        Tiers       = p.Tiers.Select(ToTierDto).ToList(),
    };

    private static RewardTierDto ToTierDto(RewardTier t) => new()
    {
        Id                = t.Id,
        RewardProgramId   = t.RewardProgramId,
        Label             = t.Label,
        MinInvoiceAmount  = t.MinInvoiceAmount,
        PointsForReferrer = t.PointsForReferrer,
        GiftCardAmount    = t.GiftCardAmount,
        IsActive          = t.IsActive,
        DisplayOrder      = t.DisplayOrder,
    };
}
