using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class ContactService(IContactRepository repo) : IContactService
{
    public async Task<IReadOnlyList<ContactDto>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        var contacts = string.IsNullOrWhiteSpace(search)
            ? await repo.GetAllAsync(ct)
            : await repo.SearchAsync(search, ct);

        return contacts.Select(c => new ContactDto
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            Phone = c.Phone,
            Company = c.Company,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<ContactDto> CreateAsync(ContactDto dto, CancellationToken ct = default)
    {
        var contact = new Contact
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Company = dto.Company
        };

        await repo.AddAsync(contact, ct);
        await repo.SaveChangesAsync(ct);
        dto.Id = contact.Id;
        return dto;
    }
}
