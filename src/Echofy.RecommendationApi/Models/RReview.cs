namespace Echofy.RecommendationApi.Models;

public class RReview
{
    public int Id { get; set; }
    public string AppUserId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int Rating { get; set; }
}
