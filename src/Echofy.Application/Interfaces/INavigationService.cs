using System.Security.Claims;
using Echofy.Application.DTOs;

namespace Echofy.Application.Interfaces;

public interface INavigationService
{
    IReadOnlyList<NavMenuGroupDto> GetMenu(ClaimsPrincipal user);
}
