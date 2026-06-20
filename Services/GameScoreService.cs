using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

public sealed class GameScoreService
{
    private readonly IGameScoreRepository _scores;

    public GameScoreService(IGameScoreRepository scores)
    {
        _scores = scores;
    }

    public async Task<GameScoreListDto> GetByPlayerAsync(string playerId)
    {
        IReadOnlyList<GameScore> scores = await _scores.GetByPlayerAsync(playerId);
        return new GameScoreListDto(scores.Select(score => score.ToDto()).ToList());
    }

    public async Task<GameScoreDto> RecordAsync(
        RecordGameScoreRequestDto request,
        string requestingPlayerId)
    {
        if (!request.Players.Contains(requestingPlayerId))
        {
            request.Players.Add(requestingPlayerId);
        }

        var score = new GameScore
        {
            MatchId = request.MatchId.Trim(),
            Players = request.Players.Distinct().ToList(),
            EscapeTimeSeconds = request.EscapeTimeSeconds,
            Result = request.Result.Trim()
        };
        await _scores.CreateAsync(score);
        return score.ToDto();
    }
}
