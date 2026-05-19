using System.Security.Claims;
using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Echofy.Infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string? UserId =>
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
