using Echofy.MobileApp.Models;

namespace Echofy.MobileApp.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
