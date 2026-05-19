using System.ComponentModel.DataAnnotations.Schema;

namespace Echofy.Domain.Entities;

public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }

    [NotMapped] public decimal Amount => Math.Round(Quantity * UnitPrice, 2);
}
