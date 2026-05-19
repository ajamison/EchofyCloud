using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/deals")]
[Authorize]
public class DealsController(IDealService dealService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await dealService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var deal = await dealService.GetByIdAsync(id, ct);
        return deal is null ? NotFound() : Ok(deal);
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics(CancellationToken ct)
        => Ok(await dealService.GetAnalyticsAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DealDto dto, CancellationToken ct)
    {
        var created = await dealService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DealDto dto, CancellationToken ct)
    {
        var updated = await dealService.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }
}
