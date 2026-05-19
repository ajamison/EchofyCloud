using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
}
