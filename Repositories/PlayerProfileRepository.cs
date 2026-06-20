using API.Config;
using API.Models;
using API.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.Repositories;

public interface IPlayerProfileRepository
{
    Task<PlayerProfile?> GetByUserIdAsync(string userId);
    Task CreateAsync(PlayerProfile profile);
    Task UpdateNicknameAsync(string userId, string nickname);
    Task IncrementStatsAsync(string userId, bool escaped);
}

// [GNS301_Require] Repository Pattern giữ MongoDB CRUD của profile trong Data Access layer.
public sealed class PlayerProfileRepository : IPlayerProfileRepository
{
    private readonly IMongoCollection<PlayerProfile> _profiles;

    public PlayerProfileRepository(MongoDbService database, IOptions<MongoDbSettings> settings)
    {
        _profiles = database.GetCollection<PlayerProfile>(
            settings.Value.PlayerProfilesCollectionName);
    }

    public async Task<PlayerProfile?> GetByUserIdAsync(string userId) =>
        await _profiles.Find(profile => profile.UserId == userId).FirstOrDefaultAsync();

    public async Task CreateAsync(PlayerProfile profile) =>
        await _profiles.InsertOneAsync(profile);

    public async Task UpdateNicknameAsync(string userId, string nickname) =>
        await _profiles.UpdateOneAsync(
            profile => profile.UserId == userId,
            Builders<PlayerProfile>.Update.Set(profile => profile.Nickname, nickname));

    public async Task IncrementStatsAsync(string userId, bool escaped)
    {
        var update = escaped
            ? Builders<PlayerProfile>.Update.Inc(profile => profile.Escapes, 1)
            : Builders<PlayerProfile>.Update.Inc(profile => profile.Fails, 1);
        await _profiles.UpdateOneAsync(profile => profile.UserId == userId, update);
    }
}
