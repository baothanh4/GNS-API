using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "Player")]
[Route("api/gamescores")]
public sealed class GameScoresController : ControllerBase
{
    private readonly GameScoreService _scores;

    public GameScoresController(GameScoreService scores)
    {
        _scores = scores;
    }

    [HttpGet("me")]
    public async Task<ActionResult<GameScoreListDto>> GetMine()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null
            ? Unauthorized()
            : Ok(await _scores.GetByPlayerAsync(userId));
    }

    [HttpPost]
    public async Task<ActionResult<GameScoreDto>> Record(
        RecordGameScoreRequestDto request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null
            ? Unauthorized()
            : Ok(await _scores.RecordAsync(request, userId));
    }
}
