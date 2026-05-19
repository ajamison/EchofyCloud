namespace Echofy.RecommendationApi.Models;

public class RProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}
