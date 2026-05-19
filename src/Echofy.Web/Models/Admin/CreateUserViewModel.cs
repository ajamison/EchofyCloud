using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Echofy.Web.Models.Admin;

public class CreateUserViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string SelectedRole { get; set; } = string.Empty;
    public int? ClientId { get; set; }

    public List<SelectListItem> RoleOptions { get; set; } = [];
    public List<SelectListItem> ClientOptions { get; set; } = [];
}
