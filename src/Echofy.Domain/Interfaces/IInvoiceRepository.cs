using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetAllAsync(int? clientId, int? companyId = null, CancellationToken ct = default);
    Task<Invoice?> GetByIdAsync(int id, int? clientId, CancellationToken ct = default);
    Task<List<Invoice>> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Invoice?> GetByIdForCustomerAsync(int id, string email, CancellationToken ct = default);
    Task<string> GenerateInvoiceNumberAsync(CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    void Update(Invoice invoice);
    void Remove(Invoice invoice);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
