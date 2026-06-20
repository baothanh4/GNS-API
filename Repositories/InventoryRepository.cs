using API.Config;
using API.Models;
using API.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace API.Repositories;

public interface IInventoryRepository
{
    Task<Inventory?> GetByPlayerIdAsync(string playerId);
    Task CreateAsync(Inventory inventory);
    Task ReplaceItemsAsync(string playerId, IReadOnlyList<InventoryItem> items);
    Task AddItemAsync(string playerId, InventoryItem item);
}

// [GNS301_Require] Repository Pattern tập trung mọi thao tác MongoDB của inventory.
public sealed class InventoryRepository : IInventoryRepository
{
    private readonly IMongoCollection<Inventory> _inventories;

    public InventoryRepository(MongoDbService database, IOptions<MongoDbSettings> settings)
    {
        _inventories = database.GetCollection<Inventory>(
            settings.Value.InventoriesCollectionName);
    }

    public async Task<Inventory?> GetByPlayerIdAsync(string playerId) =>
        await _inventories.Find(inventory => inventory.PlayerId == playerId)
            .FirstOrDefaultAsync();

    public async Task CreateAsync(Inventory inventory) =>
        await _inventories.InsertOneAsync(inventory);

    public async Task ReplaceItemsAsync(string playerId, IReadOnlyList<InventoryItem> items) =>
        await _inventories.UpdateOneAsync(
            inventory => inventory.PlayerId == playerId,
            Builders<Inventory>.Update.Set(
                inventory => inventory.Items,
                items.ToList()));

    public async Task AddItemAsync(string playerId, InventoryItem item)
    {
        Inventory? inventory = await GetByPlayerIdAsync(playerId);
        if (inventory is null)
        {
            return;
        }

        InventoryItem? existing = inventory.Items.Find(entry => entry.ItemId == item.ItemId);
        if (existing is null)
        {
            inventory.Items.Add(item);
        }
        else
        {
            existing.Quantity += item.Quantity;
        }

        await ReplaceItemsAsync(playerId, inventory.Items);
    }
}
