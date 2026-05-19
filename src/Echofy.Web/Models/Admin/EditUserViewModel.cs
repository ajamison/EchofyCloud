using Microsoft.AspNetCore.Mvc.Rendering;

namespace Echofy.Web.Models.Admin;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SelectedRole { get; set; } = string.Empty;
    public int? ClientId { get; set; }

    public List<SelectListItem> RoleOptions { get; set; } = [];
    public List<SelectListItem> ClientOptions { get; set; } = [];
}
