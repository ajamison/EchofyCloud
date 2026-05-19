using System.ComponentModel.DataAnnotations;

namespace Echofy.Web.Models.Admin;

public class ManufacturerViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Url]
    public string? Website { get; set; }

    public bool IsActive { get; set; } = true;

    // List view only
    public int ProductCount { get; set; }
}
