using Echofy.Application.DTOs;

namespace Echofy.Web.Models;

public class ProductPageViewModel
{
    public ProductDto Product { get; set; } = null!;
    public IReadOnlyList<ReviewDto> Reviews { get; set; } = [];
    public double AverageRating { get; set; }
    public bool IsFavorited { get; set; }
    public bool UserHasReviewed { get; set; }
}
