using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IContactService
{
    Task<IReadOnlyList<ContactDto>> GetAllAsync(string? search = null, CancellationToken ct = default);
    Task<ContactDto> CreateAsync(ContactDto dto, CancellationToken ct = default);
}
