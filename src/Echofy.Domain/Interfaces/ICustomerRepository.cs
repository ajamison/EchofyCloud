using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<IReadOnlyList<Customer>> GetAllAsync(int? clientId, string? search, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(int id, int? clientId, CancellationToken ct = default);
}
