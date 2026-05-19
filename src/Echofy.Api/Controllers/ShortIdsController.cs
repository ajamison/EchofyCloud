using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/short-ids")]
[Authorize(Roles = "Admin,Manager")]
public class ShortIdsController(IProductService productService) : ControllerBase
{
    /// <summary>Returns all QR label codes, optionally filtered by assignment status.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? assigned, CancellationToken ct)
        => Ok(await productService.GetAllShortIdsAsync(assigned, ct));

    /// <summary>Generate a batch of unassigned QR codes ready for printing.</summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateBatch([FromBody] GenerateBatchRequest req, CancellationToken ct)
        => Ok(await productService.GenerateBatchShortIdsAsync(req.Count, req.Label, ct));

    /// <summary>Assign a product to a previously unassigned code.</summary>
    [HttpPut("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignProductRequest req, CancellationToken ct)
    {
        var result = await productService.AssignProductAsync(id, req.ProductId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Remove the product assignment from a code (returns it to the unassigned pool).</summary>
    [HttpPut("{id:int}/unassign")]
    public async Task<IActionResult> Unassign(int id, CancellationToken ct)
    {
        var ok = await productService.UnassignShortIdAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Permanently delete a QR label code.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ok = await productService.DeleteShortIdAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }
}

public record GenerateBatchRequest(int Count, string? Label);
public record AssignProductRequest(int ProductId);
