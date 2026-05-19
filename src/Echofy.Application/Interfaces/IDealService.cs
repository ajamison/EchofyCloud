using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IDealService
{
    Task<IReadOnlyList<DealDto>> GetAllAsync(CancellationToken ct = default);
    Task<DealDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DealDto> CreateAsync(DealDto dto, CancellationToken ct = default);
    Task<DealDto?> UpdateAsync(int id, DealDto dto, CancellationToken ct = default);
    Task<CrmAnalyticsDto> GetAnalyticsAsync(CancellationToken ct = default);
}
