using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public record CreateInvoiceRequest(
    int? CompanyId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? AppUserId,
    DateTime IssuedDate,
    DateTime DueDate,
    string? Notes,
    decimal TotalAmount);

public record UpdateInvoiceRequest(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? AppUserId,
    DateTime IssuedDate,
    DateTime DueDate,
    string? Notes,
    decimal TotalAmount);

public interface IInvoiceService
{
    Task<List<InvoiceListItemDto>> GetAllAsync(int? clientId, int? companyId = null, CancellationToken ct = default);
    Task<InvoiceDto?> GetByIdAsync(int id, int? clientId, CancellationToken ct = default);
    Task<List<InvoiceListItemDto>> GetForCustomerAsync(string email, CancellationToken ct = default);
    Task<InvoiceDto?> GetForCustomerByIdAsync(int id, string email, CancellationToken ct = default);
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest req, string createdByUserId, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, int? clientId, UpdateInvoiceRequest req, CancellationToken ct = default);
    Task<bool> SendAsync(int id, int? clientId, CancellationToken ct = default);
    Task<bool> MarkPaidAsync(int id, int? clientId, CancellationToken ct = default);
    Task<bool> CancelAsync(int id, int? clientId, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, int? clientId, CancellationToken ct = default);
}
