using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize(Roles = "Player")]
[Route("api/inventories")]
public sealed class InventoriesController : ControllerBase
{
    private readonly InventoryService _inventories;

    public InventoriesController(InventoryService inventories)
    {
        _inventories = inventories;
    }

    [HttpGet("me")]
    public async Task<ActionResult<InventoryDto>> GetMine()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        InventoryDto? inventory = userId is null
            ? null
            : await _inventories.GetByPlayerIdAsync(userId);
        return inventory is null ? NotFound() : Ok(inventory);
    }

    [HttpPut("me")]
    public async Task<IActionResult> ReplaceMine(UpdateInventoryRequestDto request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }
        await _inventories.ReplaceItemsAsync(userId, request);
        return NoContent();
    }

    [HttpPost("me/items")]
    public async Task<IActionResult> AddItem(AddInventoryItemRequestDto request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }
        await _inventories.AddItemAsync(userId, request);
        return NoContent();
    }
}
