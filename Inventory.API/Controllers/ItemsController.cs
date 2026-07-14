using Inventory.Core.Classes;
using Inventory.API.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _service;

        public ItemsController(IItemService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemUniversal>>> Get()
        {
            var items = await _service.ListItemAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemUniversal>> GetById(string id)
        {
            var item = await _service.FindByIdAsync(id);
            if (item == null) return NotFound($"Item with ID {id} not found.");
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ItemUniversal item)
        {
            var created = await _service.RegisterItemAsync(item);
            if (!created) return BadRequest("Could not register the item. Check the data or look for a duplicate SKU.");

            return CreatedAtAction(nameof(GetById), new { id = item.SKU }, item);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ItemUniversal item)
        {
            var updated = await _service.ModifyItemAsync(item);
            if (!updated) return NotFound($"Could not update. Item with ID {item.Id} does not exist.");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _service.RemoveItemAsync(id);
            if (!deleted) return NotFound($"Item with ID {id} does not exist.");
            return NoContent();
        }

        [HttpGet("total-value")]
        public async Task<ActionResult<decimal>> GetTotalValue()
        {
            var total = await _service.GetTotalInventoryValueAsync();
            return Ok(total);
        }
    }
}
