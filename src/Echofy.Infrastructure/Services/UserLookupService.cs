using Echofy.Application.Interfaces;
using Echofy.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Echofy.Infrastructure.Services;

public class UserLookupService(UserManager<AppUser> userManager) : IUserLookupService
{
    public async Task<string?> FindUserIdByEmailAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user?.Id;
    }
}
