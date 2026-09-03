using Inventory.API.Services;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TerminalsController : ControllerBase
    {
        private readonly ITerminalService _service;

        public TerminalsController(ITerminalService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? customerAccountId)
        {
            var terminals = await _service.ListAsync(customerAccountId);
            return Ok(terminals);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var terminal = await _service.FindByIdAsync(id);
            if (terminal == null) return NotFound();
            return Ok(terminal);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Terminal terminal)
        {
            if (terminal == null) return BadRequest("Caja inválida.");

            var (created, error) = await _service.CreateAsync(terminal);
            if (error != null) return BadRequest(error);

            return CreatedAtAction(nameof(GetById), new { id = created!.Id }, created);
        }

        [HttpGet("{id}/payment-methods")]
        public async Task<IActionResult> GetPaymentMethods(int id)
            => Ok(await _service.ListPaymentMethodsAsync(id));

        [HttpPut("{id}/payment-methods")]
        public async Task<IActionResult> SavePaymentMethods(int id, [FromBody] List<TerminalPaymentMethod> methods)
        {
            var result = await _service.SavePaymentMethodsAsync(id, methods ?? new());
            if (!result.Success) return BadRequest(result.Error);
            return NoContent();
        }

        [HttpGet("{id}/ranges")]
        public async Task<IActionResult> GetRanges(int id, [FromQuery] InvoiceDocumentType? documentType)
        {
            var terminal = await _service.FindByIdAsync(id);
            if (terminal == null) return NotFound();

            var ranges = await _service.ListRangesAsync(id, documentType);
            return Ok(ranges);
        }

        /// <summary>
        /// Reserva un bloque de números para la caja. Es lo que el POS pide mientras
        /// tiene conexión para poder seguir facturando cuando la pierda.
        /// </summary>
        [HttpPost("{id}/ranges")]
        public async Task<IActionResult> AssignRange(int id, [FromBody] RangeAssignmentRequest request)
        {
            if (request == null) return BadRequest("Solicitud inválida.");

            var (range, error) = await _service.AssignRangeAsync(id, request);
            if (error != null) return BadRequest(error);

            return Ok(range);
        }
    }
}
