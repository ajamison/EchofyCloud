namespace Echofy.Domain.Entities;

public class FavoriteProduct
{
    public string AppUserId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}
