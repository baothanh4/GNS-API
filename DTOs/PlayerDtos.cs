using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public sealed record PlayerProfileDto(
    string Id,
    string UserId,
    string Nickname,
    int Level,
    int Escapes,
    int Fails);

public sealed class UpdateProfileRequestDto
{
    [Required, MinLength(3), MaxLength(24)]
    public string Nickname { get; set; } = string.Empty;
}

public sealed class MatchStatRequestDto
{
    public bool Escaped { get; set; }
}
