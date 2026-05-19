namespace Echofy.Domain.Entities;

public class ManufacturerProductImage
{
    public int Id { get; set; }
    public int ManufacturerProductId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public ManufacturerProduct ManufacturerProduct { get; set; } = null!;
}
