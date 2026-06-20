using API.DTOs;
using API.Models;
using API.Repositories;

namespace API.Services;

public sealed class InventoryService
{
    private readonly IInventoryRepository _inventories;

    public InventoryService(IInventoryRepository inventories)
    {
        _inventories = inventories;
    }

    public async Task<InventoryDto?> GetByPlayerIdAsync(string playerId)
    {
        Inventory? inventory = await _inventories.GetByPlayerIdAsync(playerId);
        return inventory?.ToDto();
    }

    public async Task ReplaceItemsAsync(string playerId, UpdateInventoryRequestDto request)
    {
        List<InventoryItem> items = request.Items.Select(ToModel).ToList();
        await _inventories.ReplaceItemsAsync(playerId, items);
    }

    public async Task AddItemAsync(string playerId, AddInventoryItemRequestDto request) =>
        await _inventories.AddItemAsync(playerId, ToModel(request));

    private static InventoryItem ToModel(AddInventoryItemRequestDto item) =>
        new()
        {
            ItemId = item.ItemId.Trim(),
            Name = item.Name.Trim(),
            Quantity = item.Quantity
        };
}
