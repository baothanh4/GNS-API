using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserService _users;
    private readonly PlayerProfileService _profiles;
    private readonly IJwtTokenService _tokens;

    public AuthController(
        UserService users,
        PlayerProfileService profiles,
        IJwtTokenService tokens)
    {
        _users = users;
        _profiles = profiles;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        var user = await _users.RegisterAsync(request);
        return user is null
            ? Conflict(new { message = "Username already exists." })
            : StatusCode(StatusCodes.Status201Created, new
            {
                message = "Registration successful.",
                username = user.Username
            });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        var user = await _users.AuthenticateAsync(request);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        PlayerProfileDto? profile = await _profiles.GetByUserIdAsync(user.Id!);
        if (profile is null)
        {
            return Problem("Player profile does not exist.");
        }

        // [GNS301_Require] JWT được cấp sau khi xác thực thành công và dùng chung cho REST/NGO/TCP chat.
        return Ok(new AuthResponseDto(
            _tokens.CreateToken(user),
            user.Id!,
            user.Username,
            profile));
    }

    [Authorize(Roles = "Player")]
    [HttpGet("session")]
    public async Task<ActionResult<SessionDto>> Session()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = userId is null ? null : await _users.GetByIdAsync(userId);
        var profile = userId is null ? null : await _profiles.GetByUserIdAsync(userId);
        return user is null || profile is null
            ? Unauthorized()
            : Ok(new SessionDto(user.Id!, user.Username, profile));
    }
}
