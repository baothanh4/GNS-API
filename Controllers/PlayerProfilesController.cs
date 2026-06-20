using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/playerprofiles")]
    public class PlayerProfilesController : ControllerBase
    {
        private readonly PlayerProfileService _profileService;

        public PlayerProfilesController(PlayerProfileService profileService)
        {
            _profileService = profileService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _profileService.GetByUserIdAsync(userId);
            if (profile == null) return NotFound(new { message = "Không tìm thấy hồ sơ người chơi." });

            return Ok(profile);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] PlayerProfile updatedProfile)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _profileService.UpdateProfileAsync(userId, updatedProfile);
            return Ok(new { message = "Cập nhật hồ sơ người chơi thành công!" });
        }

        [Authorize]
        [HttpPost("me/stats")]
        public async Task<IActionResult> IncrementStats([FromBody] StatsIncrementRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _profileService.IncrementStatsAsync(userId, request.Escaped);
            return Ok(new { message = "Cập nhật chỉ số trận đấu thành công!" });
        }

        public class StatsIncrementRequest
        {
            public bool Escaped { get; set; }
        }
    }
}
