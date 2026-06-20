using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "Player")]
[Route("api/playerprofiles")]
public sealed class PlayerProfilesController : ControllerBase
{
    private readonly PlayerProfileService _profiles;

    public PlayerProfilesController(PlayerProfileService profiles)
    {
        _profiles = profiles;
    }

    [HttpGet("me")]
    public async Task<ActionResult<PlayerProfileDto>> GetMine()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        PlayerProfileDto? profile = userId is null
            ? null
            : await _profiles.GetByUserIdAsync(userId);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMine(UpdateProfileRequestDto request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }
        await _profiles.UpdateNicknameAsync(userId, request.Nickname);
        return NoContent();
    }

    [HttpPost("me/stats")]
    public async Task<IActionResult> IncrementStats(MatchStatRequestDto request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }
        await _profiles.IncrementStatsAsync(userId, request.Escaped);
        return NoContent();
    }
}
