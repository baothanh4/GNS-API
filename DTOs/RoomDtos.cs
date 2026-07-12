using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public sealed class CreateRoomRequestDto
{
    [Required, MinLength(3), MaxLength(40)]
    public string RoomName { get; set; } = string.Empty;

    [Range(1, 4)]
    public int MaxPlayers { get; set; } = 4;

    [Range(1024, 65535)]
    public int Port { get; set; } = 7777;
}

public sealed class UpdateRoomStatusRequestDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public sealed class UpdateRelayCodeRequestDto
{
    [Required]
    public string RelayJoinCode { get; set; } = string.Empty;
}

public sealed record RoomDto(
    string Id,
    string RoomName,
    int MaxPlayers,
    IReadOnlyList<string> CurrentPlayers,
    string HostPlayerId,
    string HostAddress,
    int Port,
    string? RelayJoinCode,
    string Status,
    DateTime CreatedAt);

public sealed record RoomListDto(IReadOnlyList<RoomDto> Rooms);
