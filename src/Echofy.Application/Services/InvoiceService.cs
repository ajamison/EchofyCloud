using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class InvoiceService(IInvoiceRepository repo) : IInvoiceService
{
    public async Task<List<InvoiceListItemDto>> GetAllAsync(int? clientId, int? companyId = null, CancellationToken ct = default)
        => (await repo.GetAllAsync(clientId, companyId, ct)).Select(ToListItem).ToList();

    public async Task<InvoiceDto?> GetByIdAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var inv = await repo.GetByIdAsync(id, clientId, ct);
        return inv is null ? null : ToDto(inv);
    }

    public async Task<List<InvoiceListItemDto>> GetForCustomerAsync(string email, CancellationToken ct = default)
        => (await repo.GetByEmailAsync(email, ct)).Select(ToListItem).ToList();

    public async Task<InvoiceDto?> GetForCustomerByIdAsync(int id, string email, CancellationToken ct = default)
    {
        var inv = await repo.GetByIdForCustomerAsync(id, email, ct);
        return inv is null ? null : ToDto(inv);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest req, string createdByUserId, CancellationToken ct = default)
    {
        var number  = await repo.GenerateInvoiceNumberAsync(ct);
        var invoice = new Invoice
        {
            CompanyId       = req.CompanyId,
            InvoiceNumber   = number,
            CustomerName    = req.CustomerName,
            CustomerEmail   = req.CustomerEmail.Trim().ToLowerInvariant(),
            CustomerPhone   = req.CustomerPhone,
            AppUserId       = req.AppUserId,
            IssuedDate      = DateTime.SpecifyKind(req.IssuedDate.Date, DateTimeKind.Utc),
            DueDate         = DateTime.SpecifyKind(req.DueDate.Date, DateTimeKind.Utc),
            Notes           = req.Notes,
            TotalAmount     = req.TotalAmount,
            Status          = InvoiceStatus.Draft,
            CreatedAt       = DateTime.UtcNow,
            CreatedByUserId = createdByUserId,
        };

        await repo.AddAsync(invoice, ct);
        await repo.SaveChangesAsync(ct);
        return ToDto(invoice);
    }

    public async Task<bool> UpdateAsync(int id, int? clientId, UpdateInvoiceRequest req, CancellationToken ct = default)
    {
        var invoice = await repo.GetByIdAsync(id, clientId, ct);
        if (invoice is null) return false;
        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled) return false;

        invoice.CustomerName  = req.CustomerName;
        invoice.CustomerEmail = req.CustomerEmail.Trim().ToLowerInvariant();
        invoice.CustomerPhone = req.CustomerPhone;
        invoice.AppUserId     = req.AppUserId;
        invoice.IssuedDate    = DateTime.SpecifyKind(req.IssuedDate.Date, DateTimeKind.Utc);
        invoice.DueDate       = DateTime.SpecifyKind(req.DueDate.Date, DateTimeKind.Utc);
        invoice.Notes         = req.Notes;
        invoice.TotalAmount   = req.TotalAmount;

        repo.Update(invoice);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SendAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var inv = await repo.GetByIdAsync(id, clientId, ct);
        if (inv is null || inv.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled) return false;
        inv.Status = InvoiceStatus.Sent;
        inv.SentAt = DateTime.UtcNow;
        repo.Update(inv);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> MarkPaidAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var inv = await repo.GetByIdAsync(id, clientId, ct);
        if (inv is null || inv.Status == InvoiceStatus.Cancelled) return false;
        inv.Status = InvoiceStatus.Paid;
        inv.PaidAt = DateTime.UtcNow;
        repo.Update(inv);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CancelAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var inv = await repo.GetByIdAsync(id, clientId, ct);
        if (inv is null || inv.Status == InvoiceStatus.Paid) return false;
        inv.Status = InvoiceStatus.Cancelled;
        repo.Update(inv);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, int? clientId, CancellationToken ct = default)
    {
        var inv = await repo.GetByIdAsync(id, clientId, ct);
        if (inv is null) return false;
        repo.Remove(inv);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    private static InvoiceListItemDto ToListItem(Invoice i) => new()
    {
        Id            = i.Id,
        CompanyId     = i.CompanyId,
        CompanyName   = i.Company?.Name,
        InvoiceNumber = i.InvoiceNumber,
        CustomerName  = i.CustomerName,
        CustomerEmail = i.CustomerEmail,
        Status        = i.Status.ToString(),
        IssuedDate    = i.IssuedDate,
        DueDate       = i.DueDate,
        Total         = i.TotalAmount,
        SentAt        = i.SentAt,
        PaidAt        = i.PaidAt,
    };

    private static InvoiceDto ToDto(Invoice i) => new()
    {
        Id            = i.Id,
        CompanyId     = i.CompanyId,
        CompanyName   = i.Company?.Name,
        InvoiceNumber = i.InvoiceNumber,
        CustomerName  = i.CustomerName,
        CustomerEmail = i.CustomerEmail,
        CustomerPhone = i.CustomerPhone,
        Status        = i.Status.ToString(),
        IssuedDate    = i.IssuedDate,
        DueDate       = i.DueDate,
        Notes                = i.Notes,
        Total                = i.TotalAmount,
        SentAt               = i.SentAt,
        PaidAt               = i.PaidAt,
        CreatedAt            = i.CreatedAt,
        RewardPointsAwarded  = i.RewardPointsAwarded,
        RewardGiftCardAmount = i.RewardGiftCardAmount,
        RewardGiftCardCode   = i.RewardGiftCardCode,
        ThankYouNote  = i.ThankYouNote is null ? null : new ThankYouNoteDto
        {
            SentAt           = i.ThankYouNote.SentAt,
            ReferralIncluded = i.ThankYouNote.ReferralIncluded,
            ReferralCode     = i.ThankYouNote.ReferralCode,
        },
    };
}
