using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class DealService(IDealRepository repo, ILeadRepository leadRepo) : IDealService
{
    public async Task<IReadOnlyList<DealDto>> GetAllAsync(CancellationToken ct = default)
    {
        var deals = await repo.GetAllAsync(ct);
        return deals.Select(d => new DealDto
        {
            Id = d.Id,
            Title = d.Title,
            LeadId = d.LeadId,
            LeadName = d.Lead?.FullName ?? string.Empty,
            Stage = d.Stage,
            Value = d.Value,
            ExpectedCloseDate = d.ExpectedCloseDate,
            CreatedAt = d.CreatedAt
        }).ToList();
    }

    public async Task<DealDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var d = await repo.GetByIdAsync(id, ct);
        if (d is null) return null;

        return new DealDto
        {
            Id = d.Id,
            Title = d.Title,
            LeadId = d.LeadId,
            LeadName = d.Lead?.FullName ?? string.Empty,
            Stage = d.Stage,
            Value = d.Value,
            ExpectedCloseDate = d.ExpectedCloseDate,
            CreatedAt = d.CreatedAt
        };
    }

    public async Task<DealDto> CreateAsync(DealDto dto, CancellationToken ct = default)
    {
        var deal = new Deal
        {
            Title = dto.Title,
            LeadId = dto.LeadId,
            Stage = dto.Stage,
            Value = dto.Value,
            ExpectedCloseDate = dto.ExpectedCloseDate
        };

        await repo.AddAsync(deal, ct);
        await repo.SaveChangesAsync(ct);
        dto.Id = deal.Id;
        return dto;
    }

    public async Task<DealDto?> UpdateAsync(int id, DealDto dto, CancellationToken ct = default)
    {
        var deal = await repo.GetByIdAsync(id, ct);
        if (deal is null) return null;

        deal.Title = dto.Title;
        deal.LeadId = dto.LeadId;
        deal.Stage = dto.Stage;
        deal.Value = dto.Value;
        deal.ExpectedCloseDate = dto.ExpectedCloseDate;

        repo.Update(deal);
        await repo.SaveChangesAsync(ct);
        return dto;
    }

    public async Task<CrmAnalyticsDto> GetAnalyticsAsync(CancellationToken ct = default)
    {
        var deals = await repo.GetAllAsync(ct);
        var leads = await leadRepo.GetAllAsync(ct);

        var totalLeads = leads.Count;
        var convertedLeads = leads.Count(l => l.Status == Domain.Enums.LeadStatus.Converted);

        return new CrmAnalyticsDto
        {
            TotalLeads = totalLeads,
            TotalDeals = deals.Count,
            TotalPipelineValue = deals.Sum(d => d.Value),
            ConversionRate = totalLeads > 0 ? Math.Round((decimal)convertedLeads / totalLeads * 100, 1) : 0,
            DealsByStage = deals.GroupBy(d => d.Stage.ToString())
                                .ToDictionary(g => g.Key, g => g.Count()),
            LeadsByStatus = leads.GroupBy(l => l.Status.ToString())
                                 .ToDictionary(g => g.Key, g => g.Count())
        };
    }
}
