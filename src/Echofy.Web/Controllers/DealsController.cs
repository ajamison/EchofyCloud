using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class DealsController(IDealService dealService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Deals";
        var deals = await dealService.GetAllAsync();
        return View(deals);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Deals";
        var deal = await dealService.GetByIdAsync(id);
        if (deal is null) return NotFound();
        return View(deal);
    }

    [Authorize(Roles = "Admin,Manager,Sales,SuperAdmin")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["ActivePage"] = "Deals";
        return View(new DealDto());
    }

    [Authorize(Roles = "Admin,Manager,Sales,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DealDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await dealService.CreateAsync(dto);
        return RedirectToAction(nameof(Index));
    }
}
