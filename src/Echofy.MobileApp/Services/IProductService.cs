using Echofy.MobileApp.Models;

namespace Echofy.MobileApp.Services;

public interface IProductService
{
    Task<ProductDto?> GetByUpcAsync(string upc, CancellationToken ct = default);
}
