namespace Echofy.Api.Models;

public class LoginResponse
{
    public string       Token      { get; set; } = string.Empty;
    public string       Email      { get; set; } = string.Empty;
    public string       FullName   { get; set; } = string.Empty;
    public string       Role       { get; set; } = string.Empty;
    public List<string> Modules    { get; set; } = [];
    public int?         ClientId   { get; set; }
    public string?      ClientName { get; set; }
    public DateTime     Expires    { get; set; }
}
