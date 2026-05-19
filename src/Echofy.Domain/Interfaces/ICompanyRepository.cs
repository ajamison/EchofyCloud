using Echofy.Domain.Entities;

namespace Echofy.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<List<Company>> GetAllAsync(int? clientId = null, CancellationToken ct = default);
    Task<Company?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Company company, CancellationToken ct = default);
    void Update(Company company);
    void Remove(Company company);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
