using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IThankYouNoteRepository
{
    Task<ThankYouNote?> GetByInvoiceIdAsync(int invoiceId, CancellationToken ct = default);
    Task AddAsync(ThankYouNote note, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
