using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/inventories")]
    public class InventoriesController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoriesController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyInventory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var inventory = await _inventoryService.GetByPlayerIdAsync(userId);
            if (inventory == null) return NotFound(new { message = "Không tìm thấy túi đồ của người chơi." });

            return Ok(inventory);
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyInventory([FromBody] List<InventoryItem> items)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _inventoryService.UpdateInventoryAsync(userId, items);
            return Ok(new { message = "Cập nhật túi đồ thành công!" });
        }

        [Authorize]
        [HttpPost("me/items")]
        public async Task<IActionResult> AddItemToInventory([FromBody] InventoryItem item)
        {
            if (string.IsNullOrEmpty(item.ItemId) || string.IsNullOrEmpty(item.Name) || item.Quantity <= 0)
            {
                return BadRequest(new { message = "Thông tin vật phẩm không hợp lệ." });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _inventoryService.AddItemAsync(userId, item);
            return Ok(new { message = "Thêm vật phẩm vào túi đồ thành công!" });
        }
    }
}
