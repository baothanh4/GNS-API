using API.Config;
using Microsoft.Extensions.Options;
using API.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace API.Services
{
    public class PlayerProfileService
    {
        private readonly IMongoCollection<PlayerProfile> _profiles;

        public PlayerProfileService(MongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _profiles = mongoDbService.GetCollection<PlayerProfile>(settings.Value.PlayerProfilesCollectionName);
        }

        public async Task<PlayerProfile?> GetByUserIdAsync(string userId) =>
            await _profiles.Find(p => p.UserId == userId).FirstOrDefaultAsync();

        public async Task UpdateProfileAsync(string userId, PlayerProfile updatedProfile)
        {
            var filter = Builders<PlayerProfile>.Filter.Eq(p => p.UserId, userId);
            var update = Builders<PlayerProfile>.Update
                .Set(p => p.Nickname, updatedProfile.Nickname)
                .Set(p => p.Level, updatedProfile.Level)
                .Set(p => p.Escapes, updatedProfile.Escapes)
                .Set(p => p.Fails, updatedProfile.Fails);

            await _profiles.UpdateOneAsync(filter, update);
        }

        public async Task IncrementStatsAsync(string userId, bool escaped)
        {
            var filter = Builders<PlayerProfile>.Filter.Eq(p => p.UserId, userId);
            var update = escaped 
                ? Builders<PlayerProfile>.Update.Inc(p => p.Escapes, 1)
                : Builders<PlayerProfile>.Update.Inc(p => p.Fails, 1);

            await _profiles.UpdateOneAsync(filter, update);
        }
    }
}
