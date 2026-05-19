using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class CalendarController : Controller
{
    public IActionResult Index()
    {
        ViewData["ActivePage"] = "Calendar";
        return View();
    }
}
