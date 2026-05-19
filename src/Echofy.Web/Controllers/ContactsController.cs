using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class ContactsController(IContactService contactService) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        ViewData["ActivePage"] = "Contacts";
        var contacts = await contactService.GetAllAsync(search);
        return View(contacts);
    }

    [Authorize(Roles = "Admin,Manager,Sales,SuperAdmin")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["ActivePage"] = "Contacts";
        return View(new ContactDto());
    }

    [Authorize(Roles = "Admin,Manager,Sales,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContactDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await contactService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }
}
