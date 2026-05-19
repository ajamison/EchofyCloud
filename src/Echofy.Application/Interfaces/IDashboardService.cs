using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
}
