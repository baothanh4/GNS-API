using API.Config;
using API.Models;
using API.Services;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByUsernameAsync(string username);
    Task CreateAsync(User user);
}

// [GNS301_Require] Repository Pattern cô lập toàn bộ truy cập collection Users khỏi business logic.
public sealed class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbService database, IOptions<MongoDbSettings> settings)
    {
        _users = database.GetCollection<User>(settings.Value.UsersCollectionName);
    }

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(user => user.Id == id).FirstOrDefaultAsync();

    public async Task<User?> GetByUsernameAsync(string username)
    {
        string value = username.Trim();
        string normalized = value.ToUpperInvariant();
        var legacyName = new BsonRegularExpression(
            $"^{Regex.Escape(value)}$",
            "i");
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Eq(user => user.NormalizedUsername, normalized),
            Builders<User>.Filter.Regex(user => user.Username, legacyName));
        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User user) => await _users.InsertOneAsync(user);
}
