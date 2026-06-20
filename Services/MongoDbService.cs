using API.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> mongoDbSettings)
        {
            var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
            _database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
        }

        public IMongoDatabase GetDatabase() => _database;

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
