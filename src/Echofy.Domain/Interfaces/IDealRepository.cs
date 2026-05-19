using Echofy.Domain.Entities;
using Echofy.Domain.Enums;

namespace Echofy.Domain.Interfaces;

public interface IDealRepository : IRepository<Deal>
{
    Task<IReadOnlyList<Deal>> GetByStageAsync(DealStage stage, CancellationToken ct = default);
    Task<IReadOnlyList<Deal>> GetByLeadAsync(int leadId, CancellationToken ct = default);
}
