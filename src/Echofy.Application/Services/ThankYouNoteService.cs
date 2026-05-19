using Echofy.Application.Interfaces;
using Echofy.Domain.Entities;
using Echofy.Domain.Enums;
using Echofy.Domain.Interfaces;

namespace Echofy.Application.Services;

public class ThankYouNoteService(
    IInvoiceRepository invoiceRepo,
    IThankYouNoteRepository noteRepo,
    IReferralRepository referralRepo,
    IUserLookupService userLookup,
    IEmailService emailService) : IThankYouNoteService
{
    private const decimal ReferrerReward  = 10m;
    private const decimal WelcomeDiscount = 5m;

    public async Task<bool> SendAsync(SendThankYouRequest req, CancellationToken ct = default)
    {
        var invoice = await invoiceRepo.GetByIdAsync(req.InvoiceId, req.ClientId, ct);
        if (invoice is null || invoice.Status == InvoiceStatus.Draft) return false;

        // Idempotent — only one thank-you per invoice
        var existing = await noteRepo.GetByInvoiceIdAsync(req.InvoiceId, ct);
        if (existing is not null) return false;

        string? referralCode = null;
        string? referralUrl  = null;

        // Referral is only earned once a customer has a paid invoice
        if (invoice.Status == InvoiceStatus.Paid)
        {
            var userId = invoice.AppUserId
                ?? await userLookup.FindUserIdByEmailAsync(invoice.CustomerEmail, ct);

            if (userId is not null)
            {
                var code = await referralRepo.GetCodeByUserIdAsync(userId, ct);
                if (code is null)
                {
                    code = new ReferralCode
                    {
                        AppUserId = userId,
                        Code      = GenerateCode(8),
                        IsActive  = true,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await referralRepo.AddCodeAsync(code, ct);
                    await referralRepo.SaveChangesAsync(ct);
                }
                referralCode = code.Code;
                referralUrl  = $"{req.BaseUrl.TrimEnd('/')}/register?ref={Uri.EscapeDataString(code.Code)}";
            }
        }

        var invoiceUrl = $"{req.BaseUrl.TrimEnd('/')}/customer/invoices/{req.InvoiceId}";
        var subject    = $"Thank you for choosing us, {invoice.CustomerName}!";
        var htmlBody   = BuildHtml(invoice.CustomerName, req.CustomMessage, invoiceUrl, referralCode, referralUrl);

        await emailService.SendAsync(invoice.CustomerEmail, invoice.CustomerName, subject, htmlBody, ct);

        await noteRepo.AddAsync(new ThankYouNote
        {
            InvoiceId        = req.InvoiceId,
            CompanyId        = invoice.CompanyId,
            CustomerEmail    = invoice.CustomerEmail,
            CustomerName     = invoice.CustomerName,
            CustomMessage    = req.CustomMessage,
            ReferralIncluded = referralCode is not null,
            ReferralCode     = referralCode,
            ReferralUrl      = referralUrl,
            SentAt           = DateTime.UtcNow,
        }, ct);

        await noteRepo.SaveChangesAsync(ct);
        return true;
    }

    private static string GenerateCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }

    private static string BuildHtml(string customerName, string? customMessage, string invoiceUrl, string? referralCode, string? referralUrl)
    {
        var referralSection = referralCode is not null
            ? $"""
              <tr><td style="padding:24px 0 8px">
                <p style="margin:0;font-size:16px;font-weight:600;color:#1e293b">
                  Refer a friend and earn rewards!
                </p>
                <p style="margin:8px 0 16px;font-size:14px;color:#64748b;line-height:1.6">
                  Share your referral code with friends and family.
                  They get <strong>${WelcomeDiscount:N0} off</strong> their first order,
                  and you earn a <strong>${ReferrerReward:N0} gift card</strong> once approved.
                </p>
                <div style="background:#eff6ff;border:2px solid #2c7be5;border-radius:10px;padding:14px 20px;display:inline-block;margin-bottom:16px">
                  <span style="font-size:22px;font-weight:700;letter-spacing:4px;color:#2c7be5">{referralCode}</span>
                </div>
                <br/>
                <a href="{referralUrl}"
                   style="display:inline-block;background:#2c7be5;color:#fff;text-decoration:none;padding:10px 22px;border-radius:8px;font-weight:600;font-size:14px;margin-right:8px">
                  Share My Referral Link
                </a>
              </td></tr>
              <tr><td style="padding-bottom:16px">
                <table style="width:100%;background:#f8fafc;border-radius:10px;padding:16px;border-collapse:collapse">
                  <tr>
                    <td style="text-align:center;padding:8px 12px">
                      <div style="font-size:22px;margin-bottom:4px">📤</div>
                      <p style="margin:0;font-size:12px;font-weight:600;color:#1e293b">1. Share Your Code</p>
                      <p style="margin:4px 0 0;font-size:11px;color:#64748b">Send your link to friends</p>
                    </td>
                    <td style="text-align:center;padding:8px 12px">
                      <div style="font-size:22px;margin-bottom:4px">🎉</div>
                      <p style="margin:0;font-size:12px;font-weight:600;color:#1e293b">2. They Save $5</p>
                      <p style="margin:4px 0 0;font-size:11px;color:#64748b">Instant discount on signup</p>
                    </td>
                    <td style="text-align:center;padding:8px 12px">
                      <div style="font-size:22px;margin-bottom:4px">🎁</div>
                      <p style="margin:0;font-size:12px;font-weight:600;color:#1e293b">3. You Earn $10</p>
                      <p style="margin:4px 0 0;font-size:11px;color:#64748b">Gift card after approval</p>
                    </td>
                  </tr>
                </table>
              </td></tr>
              """
            : string.Empty;

        var viewInvoiceSection = $"""
              <tr><td style="padding-bottom:24px;text-align:center">
                <a href="{invoiceUrl}"
                   style="display:inline-block;background:#2c7be5;color:#fff;text-decoration:none;padding:12px 28px;border-radius:8px;font-weight:600;font-size:15px">
                  View Your Invoice
                </a>
              </td></tr>
              """;

        var customSection = string.IsNullOrWhiteSpace(customMessage)
            ? string.Empty
            : $"""
              <tr><td style="padding-bottom:16px">
                <p style="margin:0;font-size:14px;color:#475569;line-height:1.7;white-space:pre-wrap">{System.Net.WebUtility.HtmlEncode(customMessage)}</p>
              </td></tr>
              """;

        return $"""
            <!DOCTYPE html>
            <html lang="en">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background:#f1f5f9;font-family:system-ui,-apple-system,sans-serif">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f1f5f9;padding:32px 0">
                <tr><td align="center">
                  <table width="600" cellpadding="0" cellspacing="0" style="background:#fff;border-radius:16px;overflow:hidden;max-width:600px">

                    <!-- Header -->
                    <tr><td style="background:#2c7be5;padding:32px 40px;text-align:center">
                      <h1 style="margin:0;color:#fff;font-size:24px;font-weight:700">Thank You!</h1>
                      <p style="margin:8px 0 0;color:#bfdbfe;font-size:14px">We truly appreciate your business</p>
                    </td></tr>

                    <!-- Body -->
                    <tr><td style="padding:32px 40px">
                      <table width="100%" cellpadding="0" cellspacing="0">
                        <tr><td style="padding-bottom:16px">
                          <p style="margin:0;font-size:16px;color:#1e293b">Hi <strong>{System.Net.WebUtility.HtmlEncode(customerName)}</strong>,</p>
                        </td></tr>
                        <tr><td style="padding-bottom:16px">
                          <p style="margin:0;font-size:14px;color:#475569;line-height:1.7">
                            Thank you for choosing our services. Your payment has been received and your invoice is ready to view.
                          </p>
                        </td></tr>
                        {customSection}
                        {viewInvoiceSection}
                        {referralSection}
                        <tr><td style="border-top:1px solid #e2e8f0;padding-top:16px">
                          <p style="margin:0;font-size:13px;color:#94a3b8">
                            With appreciation,<br/><strong>Your Service Team</strong>
                          </p>
                        </td></tr>
                      </table>
                    </td></tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }
}
