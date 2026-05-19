namespace Echofy.Web.Models.Admin;

public class EditClientViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool HasECommerce { get; set; }
    public bool HasCrm { get; set; }
    public bool HasKanban { get; set; }
    public bool HasCalendar { get; set; }
    public bool HasChat { get; set; }
    public bool IsActive { get; set; }
}
