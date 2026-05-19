using System.ComponentModel.DataAnnotations;

namespace Echofy.Web.Models.Admin;

public class UnitOfMeasureViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Abbreviation { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // List view only
    public int ProductCount { get; set; }
}
