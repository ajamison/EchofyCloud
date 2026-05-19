using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = "Admin,Manager,Sales,Support")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    private int? ClientId => int.TryParse(User.FindFirstValue("clientId"), out var id) ? id : null;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct)
        => Ok(await customerService.GetAllAsync(ClientId, search, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var customer = await customerService.GetByIdAsync(id, ClientId, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CustomerDto dto, CancellationToken ct)
    {
        var created = await customerService.CreateAsync(dto, ClientId, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerDto dto, CancellationToken ct)
    {
        var updated = await customerService.UpdateAsync(id, dto, ClientId, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await customerService.DeleteAsync(id, ClientId, ct);
        return deleted ? NoContent() : NotFound();
    }
}
