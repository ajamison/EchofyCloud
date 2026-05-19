using Echofy.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Echofy.Infrastructure.Identity;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
}
