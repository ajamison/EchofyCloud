using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class CompanyService(ICompanyRepository repo) : ICompanyService
{
    public async Task<List<CompanyListItemDto>> GetAllAsync(int? clientId = null, CancellationToken ct = default)
    {
        var companies = await repo.GetAllAsync(clientId, ct);
        return companies.Select(c => new CompanyListItemDto
        {
            Id           = c.Id,
            ClientId     = c.ClientId,
            ClientName   = c.Client?.Name,
            Name         = c.Name,
            Email        = c.Email,
            Phone        = c.Phone,
            City         = c.City,
            Country      = c.Country,
            IsActive     = c.IsActive,
            ProductCount = c.Products.Count,
            InvoiceCount = c.Invoices.Count,
        }).ToList();
    }

    public async Task<CompanyDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var c = await repo.GetByIdAsync(id, ct);
        return c is null ? null : ToDto(c);
    }

    public async Task<CompanyDto> CreateAsync(CompanyDto dto, CancellationToken ct = default)
    {
        var company = new Company
        {
            ClientId   = dto.ClientId,
            Name       = dto.Name,
            Email      = dto.Email,
            Phone      = dto.Phone,
            Website    = dto.Website,
            TaxNumber  = dto.TaxNumber,
            Address    = dto.Address,
            City       = dto.City,
            Country    = dto.Country,
            IsActive   = dto.IsActive,
            CreatedAt  = DateTime.UtcNow,
        };

        await repo.AddAsync(company, ct);
        await repo.SaveChangesAsync(ct);
        dto.Id = company.Id;
        return dto;
    }

    public async Task<bool> UpdateAsync(int id, CompanyDto dto, CancellationToken ct = default)
    {
        var company = await repo.GetByIdAsync(id, ct);
        if (company is null) return false;

        company.ClientId  = dto.ClientId;
        company.Name      = dto.Name;
        company.Email     = dto.Email;
        company.Phone     = dto.Phone;
        company.Website   = dto.Website;
        company.TaxNumber = dto.TaxNumber;
        company.Address   = dto.Address;
        company.City      = dto.City;
        company.Country   = dto.Country;
        company.IsActive  = dto.IsActive;

        repo.Update(company);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var company = await repo.GetByIdAsync(id, ct);
        if (company is null) return false;
        repo.Remove(company);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    private static CompanyDto ToDto(Company c) => new()
    {
        Id        = c.Id,
        ClientId  = c.ClientId,
        ClientName = c.Client?.Name,
        Name      = c.Name,
        Email     = c.Email,
        Phone     = c.Phone,
        Website   = c.Website,
        TaxNumber = c.TaxNumber,
        Address   = c.Address,
        City      = c.City,
        Country   = c.Country,
        IsActive  = c.IsActive,
        CreatedAt = c.CreatedAt,
    };
}
