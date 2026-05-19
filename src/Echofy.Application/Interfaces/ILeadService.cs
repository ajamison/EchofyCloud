using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface ILeadService
{
    Task<IReadOnlyList<LeadDto>> GetAllAsync(CancellationToken ct = default);
    Task<LeadDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<LeadDto> CreateAsync(LeadDto dto, CancellationToken ct = default);
    Task<LeadDto?> UpdateAsync(int id, LeadDto dto, CancellationToken ct = default);
}
