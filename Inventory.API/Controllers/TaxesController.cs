using Inventory.API.Services;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxesController : ControllerBase
    {
        private readonly ITaxService _service;

        public TaxesController(ITaxService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tax>>> Get()
        {
            var taxes = await _service.ListTaxesAsync();
            return Ok(taxes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tax>> GetById(int id)
        {
            var tax = await _service.FindByIdAsync(id);
            if (tax == null) return NotFound();
            return Ok(tax);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Tax tax)
        {
            var created = await _service.RegisterTaxAsync(tax);
            if (!created) return BadRequest("Unable to save tax. It may already exist or have invalid data.");
            return CreatedAtAction(nameof(GetById), new { id = tax.Id }, tax);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Tax tax)
        {
            if (id != tax.Id) return BadRequest("Tax ID mismatch.");

            var updated = await _service.ModifyTaxAsync(tax);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var removed = await _service.RemoveTaxAsync(id);
            if (!removed) return NotFound();
            return NoContent();
        }
    }
}
