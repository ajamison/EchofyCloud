using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Echofy.Web.Controllers;

[Authorize]
public class CustomersController(ICustomerService customerService) : Controller
{
    private int? ClientId => int.TryParse(User.FindFirstValue("clientId"), out var id) ? id : null;

    public async Task<IActionResult> Index(string? search)
    {
        ViewData["ActivePage"] = "Customers";
        var customers = await customerService.GetAllAsync(ClientId, search);
        ViewData["Search"] = search;
        return View(customers);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["ActivePage"] = "Customers";
        var customer = await customerService.GetByIdAsync(id, ClientId);
        if (customer is null) return NotFound();
        return View(customer);
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["ActivePage"] = "Customers";
        return View(new CustomerDto());
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await customerService.CreateAsync(dto, ClientId);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,Manager,Support,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActivePage"] = "Customers";
        var customer = await customerService.GetByIdAsync(id, ClientId);
        if (customer is null) return NotFound();
        return View(customer);
    }

    [Authorize(Roles = "Admin,Manager,Support,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CustomerDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await customerService.UpdateAsync(id, dto, ClientId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Manager,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await customerService.DeleteAsync(id, ClientId);
        return RedirectToAction(nameof(Index));
    }
}
