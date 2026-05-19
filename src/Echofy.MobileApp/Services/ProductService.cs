using System.Net.Http.Headers;
using System.Net.Http.Json;
using Echofy.MobileApp.Models;

namespace Echofy.MobileApp.Services;

public class ProductService(HttpClient http) : IProductService
{
    public async Task<ProductDto?> GetByUpcAsync(string upc, CancellationToken ct = default)
    {
        var token = await SecureStorage.GetAsync("jwt_token");
        if (!string.IsNullOrEmpty(token))
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        var response = await http.GetAsync($"api/products/barcode/{Uri.EscapeDataString(upc)}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Token expired or invalid.");

        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken: ct);
    }
}
