namespace Echofy.RecommendationApi.Models;

public class RCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public List<RProduct> Products { get; set; } = [];
}
