using API.DTOs;
using API.Models;
using API.Repositories;
using System.Net;

namespace API.Services;

public sealed class RoomService
{
    private static readonly HashSet<string> ValidStatuses =
        new(StringComparer.OrdinalIgnoreCase) { "Waiting", "Playing", "Finished" };

    private readonly IRoomRepository _rooms;

    public RoomService(IRoomRepository rooms)
    {
        _rooms = rooms;
    }

    public async Task<RoomListDto> GetWaitingAsync()
    {
        IReadOnlyList<Room> rooms = await _rooms.GetWaitingAsync();
        return new RoomListDto(rooms.Select(room => room.ToDto()).ToList());
    }

    public async Task<RoomDto?> GetByIdAsync(string id) =>
        (await _rooms.GetByIdAsync(id))?.ToDto();

    public async Task<RoomDto> CreateAsync(
        CreateRoomRequestDto request,
        string ownerId,
        string hostAddress)
    {
        var room = new Room
        {
            RoomName = request.RoomName.Trim(),
            MaxPlayers = request.MaxPlayers,
            HostPlayerId = ownerId,
            HostAddress = NormalizeHostAddress(hostAddress),
            Port = request.Port,
            CurrentPlayers = new List<string> { ownerId }
        };
        await _rooms.CreateAsync(room);
        return room.ToDto();
    }

    public async Task<RoomDto?> JoinAsync(string roomId, string userId)
    {
        Room? existing = await _rooms.GetByIdAsync(roomId);
        if (existing?.CurrentPlayers.Contains(userId) == true)
        {
            return existing.ToDto();
        }
        return (await _rooms.JoinAsync(roomId, userId))?.ToDto();
    }

    public async Task<RoomDto?> JoinByNameAsync(string roomName, string userId)
    {
        Room? room = await _rooms.GetByNameAsync(roomName);
        if (room is null) return null;
        if (room.CurrentPlayers.Contains(userId)) return room.ToDto();
        return (await _rooms.JoinAsync(room.Id!, userId))?.ToDto();
    }

    public async Task<bool> LeaveAsync(string roomId, string userId)
    {
        Room? room = await _rooms.LeaveAsync(roomId, userId);
        if (room is null)
        {
            return false;
        }

        if (room.CurrentPlayers.Count == 0 || room.HostPlayerId == userId)
        {
            await _rooms.DeleteAsync(roomId);
        }
        return true;
    }

    public async Task<bool> UpdateStatusAsync(string roomId, string ownerId, string status)
    {
        if (!ValidStatuses.Contains(status))
        {
            return false;
        }
        return await _rooms.UpdateStatusAsync(
            roomId,
            ownerId,
            NormalizeStatus(status));
    }

    public async Task<bool> RemoveMemberAsync(
        string roomId,
        string ownerId,
        string memberId)
    {
        Room? room = await _rooms.GetByIdAsync(roomId);
        if (room is null || room.HostPlayerId != ownerId || memberId == ownerId)
        {
            return false;
        }
        return await _rooms.LeaveAsync(roomId, memberId) is not null;
    }

    private static string NormalizeStatus(string status) =>
        char.ToUpperInvariant(status[0]) + status[1..].ToLowerInvariant();

    private static string NormalizeHostAddress(string address)
    {
        if (!IPAddress.TryParse(address, out IPAddress? parsed))
        {
            return address;
        }
        if (IPAddress.IsLoopback(parsed))
        {
            return "127.0.0.1";
        }
        return parsed.IsIPv4MappedToIPv6
            ? parsed.MapToIPv4().ToString()
            : parsed.ToString();
    }
}
