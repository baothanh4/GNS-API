using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "Player")]
[Route("api/rooms")]
public sealed class RoomsController : ControllerBase
{
    private readonly RoomService _rooms;

    public RoomsController(RoomService rooms)
    {
        _rooms = rooms;
    }

    [HttpGet]
    public async Task<ActionResult<RoomListDto>> GetWaiting() =>
        Ok(await _rooms.GetWaitingAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDto>> Get(string id)
    {
        RoomDto? room = await _rooms.GetByIdAsync(id);
        return room is null ? NotFound() : Ok(room);
    }

    [HttpPost("create")]
    public async Task<ActionResult<RoomDto>> Create(CreateRoomRequestDto request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        string address = HttpContext.Connection.RemoteIpAddress?.ToString()
            ?? "127.0.0.1";
        RoomDto room = await _rooms.CreateAsync(request, userId, address);
        return CreatedAtAction(nameof(Get), new { id = room.Id }, room);
    }

    [HttpPost("join/{id}")]
    public async Task<ActionResult<RoomDto>> Join(string id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        RoomDto? room = userId is null ? null : await _rooms.JoinAsync(id, userId);
        return room is null
            ? Conflict(new { message = "The room does not exist, is full, or has already started." })
            : Ok(room);
    }

    [HttpPost("join-by-name/{roomName}")]
    public async Task<ActionResult<RoomDto>> JoinByName(string roomName)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        RoomDto? room = userId is null ? null : await _rooms.JoinByNameAsync(roomName, userId);
        return room is null
            ? Conflict(new { message = "Room not found or is full." })
            : Ok(room);
    }

    [HttpPost("leave/{id}")]
    public async Task<IActionResult> Leave(string id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is not null && await _rooms.LeaveAsync(id, userId)
            ? NoContent()
            : NotFound();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        string id,
        UpdateRoomStatusRequestDto request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is not null &&
               await _rooms.UpdateStatusAsync(id, userId, request.Status)
            ? NoContent()
            : BadRequest(new { message = "Invalid room status or you are not the host." });
    }

    [HttpPost("{id}/remove/{memberId}")]
    public async Task<IActionResult> RemoveMember(string id, string memberId)
    {
        string? ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return ownerId is not null &&
               await _rooms.RemoveMemberAsync(id, ownerId, memberId)
            ? NoContent()
            : Forbid();
    }
}
