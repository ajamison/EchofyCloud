using Echofy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Echofy.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ReferralController(IReferralService referralService) : ControllerBase
{
    private string UserId  => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    private string BaseUrl => $"{Request.Scheme}://{Request.Host}";

    [HttpGet("me/referral")]
    public async Task<IActionResult> GetMyReferral(CancellationToken ct)
        => Ok(await referralService.GetOrCreateReferralAsync(UserId, BaseUrl, ct));

    [HttpGet("admin/referrals")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllReferrals(CancellationToken ct)
        => Ok(await referralService.GetAllForAdminAsync(ct));

    [HttpPost("admin/referrals/{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
        => await referralService.ApproveRewardAsync(id, ct) ? Ok() : BadRequest();

    [HttpPost("admin/referrals/{id:int}/issue")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Issue(int id, CancellationToken ct)
        => await referralService.IssueRewardAsync(id, ct) ? Ok() : BadRequest();

    [HttpPost("admin/referrals/{id:int}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        => await referralService.CancelRewardAsync(id, ct) ? Ok() : BadRequest();
}
