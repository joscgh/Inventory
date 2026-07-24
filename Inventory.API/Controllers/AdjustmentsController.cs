using Inventory.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/items/{sku}/[controller]")]
    public class AdjustmentsController : ControllerBase
    {
        private readonly IAdjustmentRepository _repo;

        public AdjustmentsController(IAdjustmentRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetBySku(string sku)
        {
            var list = await _repo.GetBySkuAsync(sku);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string sku, [FromBody] Inventory.Core.Classes.InventoryAdjustment adj)
        {
            if (adj == null) return BadRequest();
            adj.SKU = sku;
            adj.Timestamp = DateTime.UtcNow;
            await _repo.AddAsync(adj);
            return CreatedAtAction(nameof(GetBySku), new { sku }, adj);
        }
    }
}
