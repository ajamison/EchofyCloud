using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class LeadsController(ILeadService leadService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Leads";
        var leads = await leadService.GetAllAsync();
        return View(leads);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Leads";
        var lead = await leadService.GetByIdAsync(id);
        if (lead is null) return NotFound();
        return View(lead);
    }

    [Authorize(Roles = "Admin,Manager,Sales,SuperAdmin")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["ActivePage"] = "Leads";
        return View(new LeadDto());
    }

    [Authorize(Roles = "Admin,Manager,Sales,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeadDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await leadService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }
}
