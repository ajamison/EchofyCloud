namespace Echofy.RecommendationApi.Models;

public class RProduct
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public string ShortId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public int? ManufacturerId { get; set; }

    public List<RCategory> Categories { get; set; } = [];
    public List<RProductImage> Images { get; set; } = [];
}
