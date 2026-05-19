using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class InvoiceRepository(ApplicationDbContext db) : IInvoiceRepository
{
    public Task<List<Invoice>> GetAllAsync(int? clientId, int? companyId = null, CancellationToken ct = default)
    {
        var q = db.Invoices
            .Include(i => i.Company)
            .Include(i => i.ThankYouNote)
            .AsQueryable();

        if (clientId.HasValue)
            q = q.Where(i => i.Company != null && i.Company.ClientId == clientId.Value);

        if (companyId.HasValue)
            q = q.Where(i => i.CompanyId == companyId.Value);

        return q.OrderByDescending(i => i.CreatedAt).ToListAsync(ct);
    }

    public Task<Invoice?> GetByIdAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var q = db.Invoices
            .Include(i => i.Company)
            .Include(i => i.ThankYouNote)
            .Where(i => i.Id == id);

        if (clientId.HasValue)
            q = q.Where(i => i.Company != null && i.Company.ClientId == clientId.Value);

        return q.FirstOrDefaultAsync(ct);
    }

    public Task<List<Invoice>> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Invoices.Include(i => i.ThankYouNote)
            .Where(i => i.CustomerEmail == email.ToLowerInvariant())
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

    public Task<Invoice?> GetByIdForCustomerAsync(int id, string email, CancellationToken ct = default)
        => db.Invoices.Include(i => i.ThankYouNote)
            .FirstOrDefaultAsync(i => i.Id == id && i.CustomerEmail == email.ToLowerInvariant(), ct);

    public async Task<string> GenerateInvoiceNumberAsync(CancellationToken ct = default)
    {
        var year   = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";
        var count  = await db.Invoices.CountAsync(i => i.InvoiceNumber.StartsWith(prefix), ct);
        return $"{prefix}{(count + 1):D4}";
    }

    public async Task AddAsync(Invoice invoice, CancellationToken ct = default)
        => await db.Invoices.AddAsync(invoice, ct);

    public void Update(Invoice invoice) => db.Invoices.Update(invoice);
    public void Remove(Invoice invoice) => db.Invoices.Remove(invoice);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
