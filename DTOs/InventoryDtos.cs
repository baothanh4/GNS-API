using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public sealed record InventoryItemDto(string ItemId, string Name, int Quantity);

public sealed record InventoryDto(
    string Id,
    string PlayerId,
    IReadOnlyList<InventoryItemDto> Items);

public sealed class AddInventoryItemRequestDto
{
    [Required, MaxLength(64)]
    public string ItemId { get; set; } = string.Empty;

    [Required, MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 999)]
    public int Quantity { get; set; } = 1;
}

public sealed class UpdateInventoryRequestDto
{
    public List<AddInventoryItemRequestDto> Items { get; set; } = new();
}
