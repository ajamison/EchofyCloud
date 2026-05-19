using System.Security.Claims;
using Echofy.Application.DTOs;
using Echofy.Application.Interfaces;
using Echofy.Application.Navigation;

namespace Echofy.Application.Services;

public class NavigationService : INavigationService
{
    public IReadOnlyList<NavMenuGroupDto> GetMenu(ClaimsPrincipal user)
    {
        var result = new List<NavMenuGroupDto>();

        foreach (var group in MenuDefinition.Groups)
        {
            var visibleItems = new List<NavMenuItemDto>();

            foreach (var item in group.Items)
            {
                // Check role access
                bool hasRole = item.RequiredRoles.Any(r => user.IsInRole(r));
                if (!hasRole) continue;

                // Check module access (null RequiredModule = always accessible if role matches)
                if (item.RequiredModule is not null)
                {
                    var moduleClaim = user.FindFirst($"echofy:module:{item.RequiredModule}");
                    if (moduleClaim?.Value != "true") continue;
                }

                visibleItems.Add(new NavMenuItemDto(
                    item.Text, item.Controller, item.Action, item.ActivePage));
            }

            if (visibleItems.Count > 0)
            {
                result.Add(new NavMenuGroupDto(
                    group.Label, group.Icon, group.CollapseId, visibleItems));
            }
        }

        return result;
    }
}
