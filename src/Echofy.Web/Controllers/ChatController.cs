using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Echofy.Web.Controllers;

[Authorize]
public class ChatController : Controller
{
    public IActionResult Index()
    {
        ViewData["ActivePage"] = "Chat";
        return View();
    }
}
