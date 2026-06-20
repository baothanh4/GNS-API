using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomsController : ControllerBase
    {
        private readonly RoomService _roomService;

        public RoomsController(RoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _roomService.GetRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoom(string id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null) return NotFound(new { message = "Không tìm thấy phòng." });
            return Ok(room);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            if (string.IsNullOrEmpty(request.RoomName))
            {
                return BadRequest(new { message = "Tên phòng không được để trống." });
            }

            var room = await _roomService.CreateRoomAsync(request.RoomName, request.MaxPlayers);
            return Ok(room);
        }

        [Authorize]
        [HttpPost("join/{id}")]
        public async Task<IActionResult> JoinRoom(string id)
        {
            var playerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính người chơi." });
            }

            var success = await _roomService.JoinRoomAsync(id, playerId);
            if (!success)
            {
                return BadRequest(new { message = "Không thể tham gia phòng. Phòng có thể đã đầy hoặc không tồn tại." });
            }

            return Ok(new { message = "Tham gia phòng thành công!", roomId = id });
        }

        [Authorize]
        [HttpPost("leave/{id}")]
        public async Task<IActionResult> LeaveRoom(string id)
        {
            var playerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerId))
            {
                return Unauthorized(new { message = "Không xác định được danh tính người chơi." });
            }

            var success = await _roomService.LeaveRoomAsync(id, playerId);
            if (!success)
            {
                return BadRequest(new { message = "Không thể rời phòng." });
            }

            return Ok(new { message = "Rời phòng thành công!" });
        }

        public class CreateRoomRequest
        {
            public string RoomName { get; set; } = null!;
            public int MaxPlayers { get; set; } = 4;
        }
    }
}
