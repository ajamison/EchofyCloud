namespace Echofy.Domain.Entities;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Sku { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}
