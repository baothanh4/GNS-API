using API.Config;
using API.Models;
using API.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace API.Repositories;

public interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetWaitingAsync();
    Task<Room?> GetByIdAsync(string id);
    Task<Room?> GetByNameAsync(string roomName);
    Task CreateAsync(Room room);
    Task<Room?> JoinAsync(string roomId, string userId);
    Task<Room?> LeaveAsync(string roomId, string userId);
    Task<bool> UpdateStatusAsync(string roomId, string ownerId, string status);
    Task<bool> SetRelayCodeAsync(string roomId, string ownerId, string relayJoinCode);
    Task DeleteAsync(string roomId);
}

// [GNS301_Require] Repository Pattern đặt truy vấn phòng và update atomic trong Data Access layer.
public sealed class RoomRepository : IRoomRepository
{
    private readonly IMongoCollection<Room> _rooms;

    public RoomRepository(MongoDbService database, IOptions<MongoDbSettings> settings)
    {
        _rooms = database.GetCollection<Room>(settings.Value.RoomsCollectionName);
    }

    public async Task<IReadOnlyList<Room>> GetWaitingAsync() =>
        await _rooms.Find(room => room.Status == "Waiting")
            .SortByDescending(room => room.CreatedAt)
            .ToListAsync();

    public async Task<Room?> GetByIdAsync(string id) =>
        await _rooms.Find(room => room.Id == id).FirstOrDefaultAsync();

    public async Task<Room?> GetByNameAsync(string roomName) =>
        await _rooms.Find(room => room.RoomName.ToLower() == roomName.ToLower() && room.Status == "Waiting")
            .FirstOrDefaultAsync();

    public async Task CreateAsync(Room room) => await _rooms.InsertOneAsync(room);

    public async Task<Room?> JoinAsync(string roomId, string userId)
    {
        var capacity = new BsonDocument("$expr",
            new BsonDocument("$lt", new BsonArray
            {
                new BsonDocument("$size", "$currentPlayers"),
                "$maxPlayers"
            }));
        var filter = Builders<Room>.Filter.And(
            Builders<Room>.Filter.Eq(room => room.Id, roomId),
            Builders<Room>.Filter.Eq(room => room.Status, "Waiting"),
            new BsonDocumentFilterDefinition<Room>(capacity));
        var update = Builders<Room>.Update
            .AddToSet(room => room.CurrentPlayers, userId)
            .Set(room => room.UpdatedAt, DateTime.UtcNow);
        return await _rooms.FindOneAndUpdateAsync(
            filter,
            update,
            new FindOneAndUpdateOptions<Room, Room> { ReturnDocument = ReturnDocument.After });
    }

    public async Task<Room?> LeaveAsync(string roomId, string userId)
    {
        var update = Builders<Room>.Update
            .Pull(room => room.CurrentPlayers, userId)
            .Set(room => room.UpdatedAt, DateTime.UtcNow);
        return await _rooms.FindOneAndUpdateAsync<Room, Room>(
            room => room.Id == roomId,
            update,
            new FindOneAndUpdateOptions<Room, Room> { ReturnDocument = ReturnDocument.After });
    }

    public async Task<bool> UpdateStatusAsync(string roomId, string ownerId, string status)
    {
        UpdateResult result = await _rooms.UpdateOneAsync(
            room => room.Id == roomId && room.HostPlayerId == ownerId,
            Builders<Room>.Update
                .Set(room => room.Status, status)
                .Set(room => room.UpdatedAt, DateTime.UtcNow));
        return result.ModifiedCount > 0;
    }

    // [GNS301_Require] Repository method lưu Relay JoinCode vào MongoDB, chỉ host mới được set.
    public async Task<bool> SetRelayCodeAsync(string roomId, string ownerId, string relayJoinCode)
    {
        UpdateResult result = await _rooms.UpdateOneAsync(
            room => room.Id == roomId && room.HostPlayerId == ownerId,
            Builders<Room>.Update
                .Set(room => room.RelayJoinCode, relayJoinCode)
                .Set(room => room.UpdatedAt, DateTime.UtcNow));
        return result.ModifiedCount > 0;
    }

    public async Task DeleteAsync(string roomId) =>
        await _rooms.DeleteOneAsync(room => room.Id == roomId);
}
