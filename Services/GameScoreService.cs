using API.Config;
using Microsoft.Extensions.Options;
using API.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public class GameScoreService
    {
        private readonly IMongoCollection<GameScore> _scores;

        public GameScoreService(MongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _scores = mongoDbService.GetCollection<GameScore>(settings.Value.GameScoresCollectionName);
        }

        public async Task<List<GameScore>> GetScoresAsync() =>
            await _scores.Find(_ => true).ToListAsync();

        public async Task<List<GameScore>> GetScoresByPlayerAsync(string playerId) =>
            await _scores.Find(s => s.Players.Contains(playerId)).ToListAsync();

        public async Task<GameScore> RecordScoreAsync(GameScore score)
        {
            await _scores.InsertOneAsync(score);
            return score;
        }
    }
}
