using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerListItemDto>> GetAllAsync(int? clientId, string? search = null, CancellationToken ct = default);
    Task<CustomerDto?> GetByIdAsync(int id, int? clientId, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CustomerDto dto, int? clientId, CancellationToken ct = default);
    Task<CustomerDto?> UpdateAsync(int id, CustomerDto dto, int? clientId, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, int? clientId, CancellationToken ct = default);
}
