using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class LeadService(ILeadRepository repo) : ILeadService
{
    public async Task<IReadOnlyList<LeadDto>> GetAllAsync(CancellationToken ct = default)
    {
        var leads = await repo.GetAllAsync(ct);
        return leads.Select(l => new LeadDto
        {
            Id = l.Id,
            FullName = l.FullName,
            Email = l.Email,
            Company = l.Company,
            Phone = l.Phone,
            Status = l.Status,
            EstimatedValue = l.EstimatedValue,
            AssignedTo = l.AssignedTo,
            CreatedAt = l.CreatedAt,
            DealCount = l.Deals.Count
        }).ToList();
    }

    public async Task<LeadDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var l = await repo.GetByIdAsync(id, ct);
        if (l is null) return null;

        return new LeadDto
        {
            Id = l.Id,
            FullName = l.FullName,
            Email = l.Email,
            Company = l.Company,
            Phone = l.Phone,
            Status = l.Status,
            EstimatedValue = l.EstimatedValue,
            AssignedTo = l.AssignedTo,
            CreatedAt = l.CreatedAt,
            DealCount = l.Deals.Count
        };
    }

    public async Task<LeadDto> CreateAsync(LeadDto dto, CancellationToken ct = default)
    {
        var lead = new Lead
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Company = dto.Company,
            Phone = dto.Phone,
            Status = dto.Status,
            EstimatedValue = dto.EstimatedValue,
            AssignedTo = dto.AssignedTo
        };

        await repo.AddAsync(lead, ct);
        await repo.SaveChangesAsync(ct);
        dto.Id = lead.Id;
        return dto;
    }

    public async Task<LeadDto?> UpdateAsync(int id, LeadDto dto, CancellationToken ct = default)
    {
        var lead = await repo.GetByIdAsync(id, ct);
        if (lead is null) return null;

        lead.FullName = dto.FullName;
        lead.Email = dto.Email;
        lead.Company = dto.Company;
        lead.Phone = dto.Phone;
        lead.Status = dto.Status;
        lead.EstimatedValue = dto.EstimatedValue;
        lead.AssignedTo = dto.AssignedTo;

        repo.Update(lead);
        await repo.SaveChangesAsync(ct);
        return dto;
    }
}
