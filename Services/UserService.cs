using API.Config;
using Microsoft.Extensions.Options;
using API.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<PlayerProfile> _profiles;
        private readonly IMongoCollection<Inventory> _inventories;

        public UserService(MongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _users = mongoDbService.GetCollection<User>(settings.Value.UsersCollectionName);
            _profiles = mongoDbService.GetCollection<PlayerProfile>(settings.Value.PlayerProfilesCollectionName);
            _inventories = mongoDbService.GetCollection<Inventory>(settings.Value.InventoriesCollectionName);
        }

        public async Task<User?> GetByIdAsync(string id) =>
            await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _users.Find(u => u.Username.ToLower() == username.ToLower()).FirstOrDefaultAsync();

        public async Task<User?> RegisterAsync(string username, string password)
        {
            // Validate unique username
            var existingUser = await GetByUsernameAsync(username);
            if (existingUser != null) return null;

            var user = new User
            {
                Username = username,
                PasswordHash = HashSha256(password)
            };

            await _users.InsertOneAsync(user);

            // 1. Auto-create Player Profile
            var profile = new PlayerProfile
            {
                UserId = user.Id!,
                Nickname = user.Username, // Default nickname to username
                Level = 1,
                Escapes = 0,
                Fails = 0
            };
            await _profiles.InsertOneAsync(profile);

            // 2. Auto-create Empty Inventory
            var inventory = new Inventory
            {
                PlayerId = user.Id!,
                Items = new List<InventoryItem>()
            };
            await _inventories.InsertOneAsync(inventory);

            return user;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null) return null;

            var hash = HashSha256(password);
            if (user.PasswordHash != hash)
            {
                return null;
            }

            return user;
        }

        public static string HashSha256(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
