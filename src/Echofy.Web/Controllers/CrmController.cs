using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class CrmController(IDealService dealService) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        ViewData["ActivePage"] = "CrmDashboard";
        var analytics = await dealService.GetAnalyticsAsync();
        return View(analytics);
    }
}
