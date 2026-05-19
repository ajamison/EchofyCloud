using System.ComponentModel.DataAnnotations;

namespace Echofy.Web.Models.Admin;

public class CategoryViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // List view only
    public int ProductCount { get; set; }
}
