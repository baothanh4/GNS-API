using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

public sealed class UserService
{
    private readonly IUserRepository _users;
    private readonly IPlayerProfileRepository _profiles;
    private readonly IInventoryRepository _inventories;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IUserRepository users,
        IPlayerProfileRepository profiles,
        IInventoryRepository inventories,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _profiles = profiles;
        _inventories = inventories;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.GetByIdAsync(id);

    public async Task<User?> RegisterAsync(RegisterRequestDto request)
    {
        if (await _users.GetByUsernameAsync(request.Username) is not null)
        {
            return null;
        }

        var user = new User
        {
            Email = request.Email.Trim(),
            Username = request.Username.Trim(),
            NormalizedUsername = request.Username.Trim().ToUpperInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password)
        };
        await _users.CreateAsync(user);

        await _profiles.CreateAsync(new PlayerProfile
        {
            UserId = user.Id!,
            Nickname = user.Username
        });
        await _inventories.CreateAsync(new Inventory
        {
            PlayerId = user.Id!
        });
        return user;
    }

    public async Task<User?> AuthenticateAsync(LoginRequestDto request)
    {
        User? user = await _users.GetByUsernameAsync(request.Username);
        return user is not null &&
               _passwordHasher.Verify(request.Password, user.PasswordHash)
            ? user
            : null;
    }
}
