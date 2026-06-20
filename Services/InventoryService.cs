using API.Config;
using Microsoft.Extensions.Options;
using API.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public class InventoryService
    {
        private readonly IMongoCollection<Inventory> _inventories;

        public InventoryService(MongoDbService mongoDbService, IOptions<MongoDbSettings> settings)
        {
            _inventories = mongoDbService.GetCollection<Inventory>(settings.Value.InventoriesCollectionName);
        }

        public async Task<Inventory?> GetByPlayerIdAsync(string playerId) =>
            await _inventories.Find(i => i.PlayerId == playerId).FirstOrDefaultAsync();

        public async Task UpdateInventoryAsync(string playerId, List<InventoryItem> items)
        {
            var filter = Builders<Inventory>.Filter.Eq(i => i.PlayerId, playerId);
            var update = Builders<Inventory>.Update.Set(i => i.Items, items);
            await _inventories.UpdateOneAsync(filter, update);
        }

        public async Task AddItemAsync(string playerId, InventoryItem item)
        {
            var inventory = await GetByPlayerIdAsync(playerId);
            if (inventory == null) return;

            var existingItem = inventory.Items.Find(i => i.ItemId == item.ItemId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                inventory.Items.Add(item);
            }

            await UpdateInventoryAsync(playerId, inventory.Items);
        }
    }
}
