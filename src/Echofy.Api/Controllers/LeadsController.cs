using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/leads")]
[Authorize]
public class LeadsController(ILeadService leadService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await leadService.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var lead = await leadService.GetByIdAsync(id, ct);
        return lead is null ? NotFound() : Ok(lead);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeadDto dto, CancellationToken ct)
    {
        var created = await leadService.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LeadDto dto, CancellationToken ct)
    {
        var updated = await leadService.UpdateAsync(id, dto, ct);
        return updated is null ? NotFound() : Ok(updated);
    }
}
