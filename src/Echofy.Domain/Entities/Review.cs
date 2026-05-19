namespace Echofy.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}
