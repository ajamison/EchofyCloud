using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Echofy.Api.Controllers;

public record RewardProgramApiRequest(int ClientId, int? CompanyId, string Name, bool IsActive);
public record UpdateRewardProgramApiRequest(string Name, bool IsActive);
public record RewardTierApiRequest(string Label, decimal MinInvoiceAmount, int PointsForReferrer, decimal GiftCardAmount, bool IsActive, int DisplayOrder);

[ApiController]
[Route("api/admin/reward-programs")]
[Authorize(Roles = "Admin,SuperAdmin,SuperUser")]
public class RewardProgramController(IRewardProgramService service) : ControllerBase
{
    private int? ClientId => int.TryParse(User.FindFirstValue("clientId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? clientId, CancellationToken ct)
    {
        var effectiveClientId = clientId ?? ClientId;
        if (!effectiveClientId.HasValue)
            return BadRequest(new { message = "clientId is required" });
        return Ok(await service.GetAllForClientAsync(effectiveClientId.Value, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] RewardProgramApiRequest req, CancellationToken ct)
    {
        var result = await service.CreateAsync(
            new CreateRewardProgramRequest(req.ClientId, req.CompanyId, req.Name, req.IsActive), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRewardProgramApiRequest req, CancellationToken ct)
        => await service.UpdateAsync(id, new UpdateRewardProgramRequest(req.Name, req.IsActive), ct)
            ? NoContent()
            : NotFound();

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await service.DeleteAsync(id, ct) ? NoContent() : NotFound();

    [HttpPost("{id:int}/tiers")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> AddTier(int id, [FromBody] RewardTierApiRequest req, CancellationToken ct)
    {
        var result = await service.AddTierAsync(id,
            new SaveRewardTierRequest(req.Label, req.MinInvoiceAmount, req.PointsForReferrer, req.GiftCardAmount, req.IsActive, req.DisplayOrder), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:int}/tiers/{tierId:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateTier(int id, int tierId, [FromBody] RewardTierApiRequest req, CancellationToken ct)
        => await service.UpdateTierAsync(tierId,
            new SaveRewardTierRequest(req.Label, req.MinInvoiceAmount, req.PointsForReferrer, req.GiftCardAmount, req.IsActive, req.DisplayOrder), ct)
            ? NoContent()
            : NotFound();

    [HttpDelete("{id:int}/tiers/{tierId:int}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteTier(int id, int tierId, CancellationToken ct)
        => await service.DeleteTierAsync(tierId, ct) ? NoContent() : NotFound();
}
