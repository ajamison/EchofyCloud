using System.Net.Http.Json;
using Echofy.MobileApp.Models;

namespace Echofy.MobileApp.Services;

public class AuthService(HttpClient http) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", request, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
    }
}
