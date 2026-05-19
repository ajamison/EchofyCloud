using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class DashboardController(IDashboardService dashboardService) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewData["ActivePage"] = "Dashboard";
        var stats = await dashboardService.GetStatsAsync();
        return View(stats);
    }
}
