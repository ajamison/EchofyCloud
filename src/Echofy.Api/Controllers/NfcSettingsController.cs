using Echofy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/admin/nfc-settings")]
[Authorize(Roles = "SuperAdmin,SuperUser")]
public class NfcSettingsController(ApplicationDbContext db) : ControllerBase
{
    // ── List ───────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var clients = await db.Clients
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, HasPassword = c.NfcCardPassword != null })
            .ToListAsync(ct);

        return Ok(clients);
    }

    // ── Single client ──────────────────────────────────────────────────────────

    [HttpGet("{clientId:int}")]
    public async Task<IActionResult> Get(int clientId, CancellationToken ct)
    {
        var client = await db.Clients.FindAsync([clientId], ct);
        if (client is null) return NotFound();

        return Ok(new
        {
            ClientId   = client.Id,
            ClientName = client.Name,
            Password   = client.NfcCardPassword,
        });
    }

    // ── Set / clear password ───────────────────────────────────────────────────

    [HttpPut("{clientId:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SetPassword(int clientId, [FromBody] SetNfcPasswordRequest req, CancellationToken ct)
    {
        if (req.Password is not null && !Regex.IsMatch(req.Password, @"^[0-9A-Fa-f]{8}$"))
            return BadRequest("Password must be exactly 8 hexadecimal characters (e.g. AABBCCDD).");

        var client = await db.Clients.FindAsync([clientId], ct);
        if (client is null) return NotFound();

        client.NfcCardPassword = req.Password?.ToUpperInvariant();
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record SetNfcPasswordRequest(string? Password);
