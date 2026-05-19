namespace Echofy.MobileApp.Models;

public class LoginResponse
{
    public string   Token    { get; set; } = string.Empty;
    public string   Email    { get; set; } = string.Empty;
    public string   FullName { get; set; } = string.Empty;
    public DateTime Expires  { get; set; }
}
