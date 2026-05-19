using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class ContactsController(IContactService contactService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct)
        => Ok(await contactService.GetAllAsync(search, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ContactDto dto, CancellationToken ct)
        => Ok(await contactService.CreateAsync(dto, ct));
}
