using System.Security.Claims;
using Echofy.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Echofy.Infrastructure.Identity;

public class AppUserClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options,
    ApplicationDbContext db)
    : UserClaimsPrincipalFactory<AppUser, IdentityRole>(userManager, roleManager, options)
{
    private static readonly string[] SuperRoles = ["SuperAdmin", "SuperUser"];

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        identity.AddClaim(new Claim("echofy:fullname", user.FullName));

        if (user.ClientId.HasValue)
        {
            var client = await db.Clients.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == user.ClientId.Value);

            if (client is not null)
            {
                identity.AddClaim(new Claim("clientId", user.ClientId.Value.ToString()));
                identity.AddClaim(new Claim("echofy:client:name", client.Name));
                identity.AddClaim(new Claim("echofy:module:ecommerce", client.HasECommerce.ToString().ToLower()));
                identity.AddClaim(new Claim("echofy:module:crm", client.HasCrm.ToString().ToLower()));
                identity.AddClaim(new Claim("echofy:module:kanban", client.HasKanban.ToString().ToLower()));
                identity.AddClaim(new Claim("echofy:module:calendar", client.HasCalendar.ToString().ToLower()));
                identity.AddClaim(new Claim("echofy:module:chat", client.HasChat.ToString().ToLower()));
            }
        }
        else
        {
            // Users without a client tenant are super-level — grant access to all modules
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Any(r => SuperRoles.Contains(r)))
            {
                identity.AddClaim(new Claim("echofy:issuper", "true"));
                identity.AddClaim(new Claim("echofy:module:ecommerce", "true"));
                identity.AddClaim(new Claim("echofy:module:crm", "true"));
                identity.AddClaim(new Claim("echofy:module:kanban", "true"));
                identity.AddClaim(new Claim("echofy:module:calendar", "true"));
                identity.AddClaim(new Claim("echofy:module:chat", "true"));
            }
        }

        return identity;
    }
}
