using Echofy.Domain.Entities;
using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class LeadRepository(ApplicationDbContext db)
    : GenericRepository<Lead>(db), ILeadRepository
{
    public async Task<IReadOnlyList<Lead>> GetByStatusAsync(LeadStatus status, CancellationToken ct = default) =>
        await Db.Leads
            .Include(l => l.Deals)
            .Where(l => l.Status == status)
            .ToListAsync(ct);

    public override async Task<IReadOnlyList<Lead>> GetAllAsync(CancellationToken ct = default) =>
        await Db.Leads.Include(l => l.Deals).ToListAsync(ct);
}
