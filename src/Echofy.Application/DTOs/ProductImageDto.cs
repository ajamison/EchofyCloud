namespace Echofy.Application.DTOs;

public class ProductImageDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string? Sku { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime UploadedAt { get; set; }

    /// <summary>Relative URL for use in img src: /uploads/images/{FileName}</summary>
    public string Url => $"/uploads/images/{FileName}";
}
