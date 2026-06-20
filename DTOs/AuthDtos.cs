using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public sealed class RegisterRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(24)]
    public string Username { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(72)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthResponseDto(
    string Token,
    string UserId,
    string Username,
    PlayerProfileDto Profile);

public sealed record SessionDto(
    string UserId,
    string Username,
    PlayerProfileDto Profile);
