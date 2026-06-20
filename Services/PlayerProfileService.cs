using API.DTOs;
using API.Repositories;

namespace API.Services;

public sealed class PlayerProfileService
{
    private readonly IPlayerProfileRepository _profiles;

    public PlayerProfileService(IPlayerProfileRepository profiles)
    {
        _profiles = profiles;
    }

    public async Task<PlayerProfileDto?> GetByUserIdAsync(string userId)
    {
        var profile = await _profiles.GetByUserIdAsync(userId);
        return profile?.ToDto();
    }

    public async Task UpdateNicknameAsync(string userId, string nickname) =>
        await _profiles.UpdateNicknameAsync(userId, nickname.Trim());

    public async Task IncrementStatsAsync(string userId, bool escaped) =>
        await _profiles.IncrementStatsAsync(userId, escaped);
}
