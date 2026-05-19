using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface ICompanyService
{
    Task<List<CompanyListItemDto>> GetAllAsync(int? clientId = null, CancellationToken ct = default);
    Task<CompanyDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CompanyDto> CreateAsync(CompanyDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, CompanyDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
