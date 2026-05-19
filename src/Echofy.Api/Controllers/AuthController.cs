using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Echofy.Api.Models;
using Echofy.Application.Interfaces;
using Echofy.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IConfiguration configuration,
    IReferralService referralService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var user = new AppUser { UserName = req.Email, Email = req.Email, FullName = req.FullName };
        var result = await userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
            return BadRequest(new { message = result.Errors.First().Description });

        string? welcomeCoupon = null;
        if (!string.IsNullOrWhiteSpace(req.ReferralCode))
            welcomeCoupon = await referralService.UseReferralCodeAsync(
                req.ReferralCode.Trim().ToUpperInvariant(), user.Id, req.Email);

        return Ok(new { welcomeCoupon });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.Users
            .Include(u => u.Client)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            return Unauthorized(new { message = "Invalid credentials." });

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid credentials." });

        var jwtSection = configuration.GetSection("Jwt");
        var keyBytes   = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
        var expiryMins = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");
        var expires    = DateTime.UtcNow.AddMinutes(expiryMins);
        var roles      = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new("fullName", user.FullName),
        };

        if (user.ClientId.HasValue)
        {
            claims.Add(new Claim("clientId",   user.ClientId.Value.ToString()));
            claims.Add(new Claim("clientName", user.Client?.Name ?? ""));
        }

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var modules = new List<string>();
        if (user.Client is not null)
        {
            if (user.Client.HasECommerce) modules.Add("ecommerce");
            if (user.Client.HasCrm)       modules.Add("crm");
            if (user.Client.HasKanban)    modules.Add("kanban");
            if (user.Client.HasCalendar)  modules.Add("calendar");
            if (user.Client.HasChat)      modules.Add("chat");
        }
        else
        {
            modules.AddRange(["ecommerce", "crm", "kanban", "calendar", "chat"]);
        }
        modules.ForEach(m => claims.Add(new Claim("module", m)));

        var token = new JwtSecurityToken(
            issuer:             jwtSection["Issuer"],
            audience:           jwtSection["Audience"],
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            expires,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256));

        return Ok(new LoginResponse
        {
            Token      = new JwtSecurityTokenHandler().WriteToken(token),
            Email      = user.Email!,
            FullName   = user.FullName,
            Role       = roles.FirstOrDefault() ?? "",
            Modules    = modules,
            ClientId   = user.ClientId,
            ClientName = user.Client?.Name,
            Expires    = expires
        });
    }
}
