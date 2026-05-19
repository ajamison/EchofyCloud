namespace Echofy.Application.DTOs;

public class ProductShortIdDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Label { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AssignedAt { get; set; }
}
