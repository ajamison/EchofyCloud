using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class CustomerService(ICustomerRepository repo) : ICustomerService
{
    public async Task<IReadOnlyList<CustomerListItemDto>> GetAllAsync(int? clientId, string? search = null, CancellationToken ct = default)
    {
        var customers = await repo.GetAllAsync(clientId, search, ct);
        return customers.Select(c => new CustomerListItemDto
        {
            Id         = c.Id,
            ClientId   = c.ClientId,
            ClientName = c.Client?.Name,
            FullName   = c.FullName,
            Email      = c.Email,
            Phone      = c.Phone,
            AvatarUrl  = c.AvatarUrl,
            JoinedDate = c.JoinedDate,
        }).ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var c = await repo.GetByIdAsync(id, clientId, ct);
        if (c is null) return null;
        return ToDto(c);
    }

    public async Task<CustomerDto> CreateAsync(CustomerDto dto, int? clientId, CancellationToken ct = default)
    {
        var customer = new Customer
        {
            ClientId  = clientId ?? dto.ClientId,
            FullName  = dto.FullName,
            Email     = dto.Email.Trim().ToLowerInvariant(),
            Phone     = dto.Phone,
            AvatarUrl = dto.AvatarUrl,
            JoinedDate = DateTime.UtcNow,
            Notes     = dto.Notes,
            Address   = new Address
            {
                Street   = dto.Street,
                City     = dto.City,
                Province = dto.Province,
                Country  = dto.Country
            }
        };

        await repo.AddAsync(customer, ct);
        await repo.SaveChangesAsync(ct);
        dto.Id = customer.Id;
        dto.ClientId = customer.ClientId;
        return dto;
    }

    public async Task<CustomerDto?> UpdateAsync(int id, CustomerDto dto, int? clientId, CancellationToken ct = default)
    {
        var customer = await repo.GetByIdAsync(id, clientId, ct);
        if (customer is null) return null;

        customer.FullName        = dto.FullName;
        customer.Email           = dto.Email.Trim().ToLowerInvariant();
        customer.Phone           = dto.Phone;
        customer.Notes           = dto.Notes;
        customer.Address.Street  = dto.Street;
        customer.Address.City    = dto.City;
        customer.Address.Province = dto.Province;
        customer.Address.Country = dto.Country;

        repo.Update(customer);
        await repo.SaveChangesAsync(ct);
        return dto;
    }

    public async Task<bool> DeleteAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var customer = await repo.GetByIdAsync(id, clientId, ct);
        if (customer is null) return false;

        repo.Delete(customer);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    private static CustomerDto ToDto(Customer c) => new()
    {
        Id         = c.Id,
        ClientId   = c.ClientId,
        ClientName = c.Client?.Name,
        FullName   = c.FullName,
        Email      = c.Email,
        Phone      = c.Phone,
        AvatarUrl  = c.AvatarUrl,
        JoinedDate = c.JoinedDate,
        Notes      = c.Notes,
        Street     = c.Address.Street,
        City       = c.Address.City,
        Province   = c.Address.Province,
        Country    = c.Address.Country,
        Reviews    = []
    };
}
