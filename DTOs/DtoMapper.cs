using API.Models;

namespace API.DTOs;

public static class DtoMapper
{
    public static PlayerProfileDto ToDto(this PlayerProfile profile) =>
        new(
            profile.Id ?? string.Empty,
            profile.UserId,
            profile.Nickname,
            profile.Level,
            profile.Escapes,
            profile.Fails);

    public static InventoryDto ToDto(this Inventory inventory) =>
        new(
            inventory.Id ?? string.Empty,
            inventory.PlayerId,
            inventory.Items.Select(item => new InventoryItemDto(
                item.ItemId,
                item.Name,
                item.Quantity)).ToList());

    public static RoomDto ToDto(this Room room) =>
        new(
            room.Id ?? string.Empty,
            room.RoomName,
            room.MaxPlayers,
            room.CurrentPlayers,
            room.HostPlayerId,
            room.HostAddress,
            room.Port,
            room.Status,
            room.CreatedAt);

    public static GameScoreDto ToDto(this GameScore score) =>
        new(
            score.Id ?? string.Empty,
            score.MatchId,
            score.Players,
            score.EscapeTimeSeconds,
            score.Result,
            score.RecordedAt);
}
