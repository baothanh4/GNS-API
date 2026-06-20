using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/gamescores")]
    public class GameScoresController : ControllerBase
    {
        private readonly GameScoreService _scoreService;

        public GameScoresController(GameScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllScores()
        {
            var scores = await _scoreService.GetScoresAsync();
            return Ok(scores);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyScores()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var scores = await _scoreService.GetScoresByPlayerAsync(userId);
            return Ok(scores);
        }

        [HttpPost]
        public async Task<IActionResult> RecordScore([FromBody] GameScore score)
        {
            if (string.IsNullOrEmpty(score.MatchId) || score.Players == null || score.Players.Count == 0)
            {
                return BadRequest(new { message = "Dữ liệu lịch sử trận đấu không hợp lệ." });
            }

            var recorded = await _scoreService.RecordScoreAsync(score);
            return Ok(recorded);
        }
    }
}
