using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class KanbanController : Controller
{
    public IActionResult Index()
    {
        ViewData["ActivePage"] = "Kanban";
        return View();
    }
}
