using Echofy.Domain.Entities;
using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class DealRepository(ApplicationDbContext db)
    : GenericRepository<Deal>(db), IDealRepository
{
    public async Task<IReadOnlyList<Deal>> GetByStageAsync(DealStage stage, CancellationToken ct = default) =>
        await Db.Deals.Include(d => d.Lead).Where(d => d.Stage == stage).ToListAsync(ct);

    public async Task<IReadOnlyList<Deal>> GetByLeadAsync(int leadId, CancellationToken ct = default) =>
        await Db.Deals.Include(d => d.Lead).Where(d => d.LeadId == leadId).ToListAsync(ct);

    public override async Task<IReadOnlyList<Deal>> GetAllAsync(CancellationToken ct = default) =>
        await Db.Deals.Include(d => d.Lead).ToListAsync(ct);
}
