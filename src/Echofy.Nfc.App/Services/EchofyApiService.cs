using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Echofy.Nfc.App.Models;

namespace Echofy.Nfc.App.Services;

public class EchofyApiService : IDisposable
{
    private readonly HttpClient _http;
    private readonly string _frontendBaseUrl;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public EchofyApiService(string apiBaseUrl, string frontendBaseUrl)
    {
        _frontendBaseUrl = frontendBaseUrl.TrimEnd('/');

        // Bypass cert validation for local dev (self-signed localhost cert)
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };
        _http = new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var body = JsonSerializer.Serialize(new LoginRequest(email, password), JsonOpts);
        var res  = await _http.PostAsync("/api/auth/login",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LoginResponse>(json, JsonOpts);
    }

    public async Task<List<ProductShortIdDto>> GetAssignedShortIdsAsync(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _http.GetAsync("/api/short-ids?assigned=true");
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ProductShortIdDto>>(json, JsonOpts) ?? [];
    }

    public async Task<NfcSettingDetail?> GetNfcSettingsAsync(int clientId, string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _http.GetAsync($"/api/admin/nfc-settings/{clientId}");
        if (!res.IsSuccessStatusCode) return null;
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<NfcSettingDetail>(json, JsonOpts);
    }

    public async Task<List<ClientSummaryDto>> GetClientsAsync(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await _http.GetAsync("/api/admin/clients");
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<ClientSummaryDto>>(json, JsonOpts) ?? [];
    }

    public string BuildProductUrl(string code) => $"{_frontendBaseUrl}/p/{code}";

    public void Dispose() => _http.Dispose();
}
