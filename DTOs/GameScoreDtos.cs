using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public sealed class RecordGameScoreRequestDto
{
    [Required]
    public string MatchId { get; set; } = string.Empty;

    public List<string> Players { get; set; } = new();

    [Range(0, int.MaxValue)]
    public int EscapeTimeSeconds { get; set; }

    [Required]
    public string Result { get; set; } = string.Empty;
}

public sealed record GameScoreDto(
    string Id,
    string MatchId,
    IReadOnlyList<string> Players,
    int EscapeTimeSeconds,
    string Result,
    DateTime RecordedAt);

public sealed record GameScoreListDto(IReadOnlyList<GameScoreDto> Scores);
