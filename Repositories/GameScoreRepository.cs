using API.Config;
using API.Models;
using API.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.Repositories;

public interface IGameScoreRepository
{
    Task<IReadOnlyList<GameScore>> GetByPlayerAsync(string playerId);
    Task CreateAsync(GameScore score);
}

// [GNS301_Require] Repository Pattern cô lập lịch sử trận đấu khỏi Controller/Service.
public sealed class GameScoreRepository : IGameScoreRepository
{
    private readonly IMongoCollection<GameScore> _scores;

    public GameScoreRepository(MongoDbService database, IOptions<MongoDbSettings> settings)
    {
        _scores = database.GetCollection<GameScore>(
            settings.Value.GameScoresCollectionName);
    }

    public async Task<IReadOnlyList<GameScore>> GetByPlayerAsync(string playerId) =>
        await _scores.Find(score => score.Players.Contains(playerId))
            .SortByDescending(score => score.RecordedAt)
            .ToListAsync();

    public async Task CreateAsync(GameScore score) =>
        await _scores.InsertOneAsync(score);
}
