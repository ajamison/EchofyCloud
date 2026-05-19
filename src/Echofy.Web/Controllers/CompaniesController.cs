using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Echofy.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin,SuperUser")]
public class CompaniesController(ICompanyService companyService, ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "AdminCompanies";
        var companies = await companyService.GetAllAsync();
        return View(companies);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["ActivePage"] = "AdminCompanies";
        await PopulateClientsAsync();
        return View(new CompanyDto { IsActive = true });
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateClientsAsync();
            return View(dto);
        }

        await companyService.CreateAsync(dto);
        TempData["Success"] = "Company created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["ActivePage"] = "AdminCompanies";
        var company = await companyService.GetByIdAsync(id);
        if (company is null) return NotFound();
        await PopulateClientsAsync(company.ClientId);
        return View(company);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CompanyDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateClientsAsync(dto.ClientId);
            return View(dto);
        }

        var updated = await companyService.UpdateAsync(id, dto);
        if (!updated) return NotFound();
        TempData["Success"] = "Company updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await companyService.DeleteAsync(id);
        TempData["Success"] = "Company deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateClientsAsync(int? selectedId = null)
    {
        var clients = await db.Clients
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();

        ViewBag.Clients = new SelectList(clients, "Value", "Text", selectedId?.ToString());
    }
}
