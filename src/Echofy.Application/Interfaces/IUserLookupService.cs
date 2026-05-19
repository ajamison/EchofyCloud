namespace Echofy.Application.Interfaces;

public interface IUserLookupService
{
    Task<string?> FindUserIdByEmailAsync(string email, CancellationToken ct = default);
}
