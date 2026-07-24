using Inventory.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/inventory/history")]
    public class InventoryHistoryController : ControllerBase
    {
        private readonly IAdjustmentRepository _repo;

        public InventoryHistoryController(IAdjustmentRepository repo)
        {
            _repo = repo;
        }

        // Historial completo de ajustes de todo el inventario (más reciente primero).
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _repo.GetAllAsync();
            return Ok(list);
        }
    }
}
