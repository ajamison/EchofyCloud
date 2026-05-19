using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class CustomerRepository(ApplicationDbContext db)
    : GenericRepository<Customer>(db), ICustomerRepository
{
    public async Task<IReadOnlyList<Customer>> GetAllAsync(int? clientId, string? search, CancellationToken ct = default)
    {
        var q = Db.Customers.Include(c => c.Client).AsQueryable();

        if (clientId.HasValue)
            q = q.Where(c => c.ClientId == clientId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(c => c.FullName.Contains(search) || c.Email.Contains(search));

        return await q.OrderBy(c => c.FullName).ToListAsync(ct);
    }

    public async Task<Customer?> GetByIdAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var q = Db.Customers.Include(c => c.Client).Where(c => c.Id == id);
        if (clientId.HasValue)
            q = q.Where(c => c.ClientId == clientId.Value);
        return await q.FirstOrDefaultAsync(ct);
    }
}
