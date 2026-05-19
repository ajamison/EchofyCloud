using Echofy.Domain.Entities;
using Echofy.Domain.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Infrastructure.Repositories;

public class ThankYouNoteRepository(ApplicationDbContext db) : IThankYouNoteRepository
{
    public Task<ThankYouNote?> GetByInvoiceIdAsync(int invoiceId, CancellationToken ct = default)
        => db.ThankYouNotes.FirstOrDefaultAsync(n => n.InvoiceId == invoiceId, ct);

    public async Task AddAsync(ThankYouNote note, CancellationToken ct = default)
        => await db.ThankYouNotes.AddAsync(note, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
