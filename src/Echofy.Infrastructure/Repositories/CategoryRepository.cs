using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class CategoryRepository(ApplicationDbContext db)
    : GenericRepository<Category>(db), ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken ct = default) =>
        await Db.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<Category>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        await Db.Categories.Where(c => ids.Contains(c.Id)).ToListAsync(ct);
}
