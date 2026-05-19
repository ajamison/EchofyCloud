namespace Echofy.Application.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
