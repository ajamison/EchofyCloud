using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class CompanyRepository(ApplicationDbContext db) : ICompanyRepository
{
    public Task<List<Company>> GetAllAsync(int? clientId = null, CancellationToken ct = default)
    {
        var q = db.Companies
            .Include(c => c.Client)
            .Include(c => c.Products)
            .Include(c => c.Invoices)
            .AsQueryable();

        if (clientId.HasValue)
            q = q.Where(c => c.ClientId == clientId.Value);

        return q.OrderBy(c => c.Name).ToListAsync(ct);
    }

    public Task<Company?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Companies
            .Include(c => c.Client)
            .Include(c => c.Products)
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Company company, CancellationToken ct = default)
        => await db.Companies.AddAsync(company, ct);

    public void Update(Company company) => db.Companies.Update(company);
    public void Remove(Company company) => db.Companies.Remove(company);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
