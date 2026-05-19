using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Echofy.Api.Controllers;

public record SendThankYouApiRequest(string? CustomMessage);

public record CreateInvoiceApiRequest(
    int? CompanyId,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? AppUserId,
    DateTime IssuedDate,
    DateTime DueDate,
    string? Notes,
    decimal TotalAmount);

public record UpdateInvoiceApiRequest(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string? AppUserId,
    DateTime IssuedDate,
    DateTime DueDate,
    string? Notes,
    decimal TotalAmount);

[ApiController]
[Route("api")]
[Authorize]
public class InvoiceController(
    IInvoiceService invoiceService,
    IThankYouNoteService thankYouService,
    IRewardProgramService rewardProgramService,
    IConfiguration configuration) : ControllerBase
{
    private string FrontendBaseUrl
        => configuration["App:FrontendBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
    private string UserId    => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string UserEmail => User.FindFirstValue(ClaimTypes.Email)
                             ?? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")
                             ?? string.Empty;

    private int? ClientId => User.IsInRole("Admin")
        ? null
        : int.TryParse(User.FindFirstValue("clientId"), out var id) ? id : null;

    // ── Admin / Manager endpoints ─────────────────────────────────────────────

    [HttpGet("admin/invoices")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await invoiceService.GetAllAsync(ClientId, ct: ct));

    [HttpGet("admin/invoices/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await invoiceService.GetByIdAsync(id, ClientId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("admin/invoices")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceApiRequest req, CancellationToken ct)
    {
        var serviceReq = new CreateInvoiceRequest(
            req.CompanyId,
            req.CustomerName, req.CustomerEmail, req.CustomerPhone, req.AppUserId,
            req.IssuedDate, req.DueDate, req.Notes, req.TotalAmount);

        var result = await invoiceService.CreateAsync(serviceReq, UserId, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("admin/invoices/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInvoiceApiRequest req, CancellationToken ct)
    {
        var serviceReq = new UpdateInvoiceRequest(
            req.CustomerName, req.CustomerEmail, req.CustomerPhone, req.AppUserId,
            req.IssuedDate, req.DueDate, req.Notes, req.TotalAmount);

        return await invoiceService.UpdateAsync(id, ClientId, serviceReq, ct) ? NoContent() : BadRequest();
    }

    [HttpPost("admin/invoices/{id:int}/send")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Send(int id, CancellationToken ct)
        => await invoiceService.SendAsync(id, ClientId, ct) ? Ok() : BadRequest();

    [HttpPost("admin/invoices/{id:int}/mark-paid")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> MarkPaid(int id, CancellationToken ct)
    {
        if (!await invoiceService.MarkPaidAsync(id, ClientId, ct))
            return BadRequest();

        _ = rewardProgramService.ApplyToInvoiceAsync(id, ct);
        _ = thankYouService.SendAsync(
            new SendThankYouRequest(id, ClientId, null, FrontendBaseUrl), ct);

        return Ok();
    }

    [HttpPost("admin/invoices/{id:int}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        => await invoiceService.CancelAsync(id, ClientId, ct) ? Ok() : BadRequest();

    [HttpDelete("admin/invoices/{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await invoiceService.DeleteAsync(id, ClientId, ct) ? NoContent() : NotFound();

    [HttpPost("admin/invoices/{id:int}/send-thank-you")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SendThankYou(int id, [FromBody] SendThankYouApiRequest req, CancellationToken ct)
    {
        var result = await thankYouService.SendAsync(
            new SendThankYouRequest(id, ClientId, req.CustomMessage, FrontendBaseUrl), ct);
        return result ? Ok() : BadRequest(new { message = "Thank you note already sent or invoice not found." });
    }

    // ── Customer endpoints ────────────────────────────────────────────────────

    [HttpGet("me/invoices")]
    public async Task<IActionResult> GetMyInvoices(CancellationToken ct)
        => Ok(await invoiceService.GetForCustomerAsync(UserEmail, ct));

    [HttpGet("me/invoices/{id:int}")]
    public async Task<IActionResult> GetMyInvoice(int id, CancellationToken ct)
    {
        var result = await invoiceService.GetForCustomerByIdAsync(id, UserEmail, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
