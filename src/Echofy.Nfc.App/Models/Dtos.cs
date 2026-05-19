namespace Echofy.Nfc.App.Models;

public class NfcAuthException(string message) : Exception(message);

public record LoginRequest(string Email, string Password);

public record LoginResponse(
    string Token,
    string Email,
    string FullName,
    string Role,
    List<string> Modules,
    int? ClientId,
    string? ClientName,
    DateTime Expires);

public record ProductShortIdDto(
    int Id,
    int? ProductId,
    string? ProductName,
    string Code,
    string? Label,
    string CreatedAt,
    string? AssignedAt);

public record NfcSettingDetail(int ClientId, string ClientName, string? Password);

public record ClientSummaryDto(int Id, string Name, bool IsActive);

public record AppConfig
{
    public string ApiBaseUrl { get; init; } = "https://localhost:58269";
    public string FrontendBaseUrl { get; init; } = "http://localhost:5173";
    /// <summary>Client whose NFC card password is fetched after SuperAdmin/SuperUser login.</summary>
    public int? ClientId { get; init; }
}
