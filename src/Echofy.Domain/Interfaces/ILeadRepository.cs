using Echofy.Domain.Entities;
using Echofy.Domain.Enums;

namespace Echofy.Domain.Interfaces;

public interface ILeadRepository : IRepository<Lead>
{
    Task<IReadOnlyList<Lead>> GetByStatusAsync(LeadStatus status, CancellationToken ct = default);
}
