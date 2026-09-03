using Inventory.API.Services;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _service;

        public InvoicesController(IInvoiceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] InvoiceDocumentType? documentType,
            [FromQuery] int? terminalId,
            [FromQuery] int? customerAccountId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var invoices = await _service.ListAsync(documentType, terminalId, customerAccountId, from, to);
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _service.FindByIdAsync(id);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        /// <summary>
        /// Registra una factura, venga de una emisión en línea (sin número, lo asigna
        /// el servidor) o de una caja que la emitió sin conexión (ya trae número de su
        /// rango reservado). Es idempotente por ClientGuid.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Invoice invoice)
        {
            if (invoice == null) return BadRequest("Factura inválida.");

            var result = await _service.RegisterAsync(invoice);

            if (result.Error != null) return BadRequest(result.Error);
            if (result.Invoice == null) return BadRequest("No se pudo registrar la factura.");

            // 200 en vez de 201 cuando ya existía: le dice al POS "ya la tenía, saca
            // esto de la cola" sin que tenga que interpretar un error.
            if (!result.Created)
            {
                return Ok(result.Invoice);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Invoice.Id }, result.Invoice);
        }

        /// <summary>
        /// Sube en bloque la cola de una caja que estuvo sin conexión. Cada factura se
        /// procesa por separado: una que falle no arrastra a las demás.
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> Sync([FromBody] List<Invoice> invoices)
        {
            if (invoices == null || invoices.Count == 0)
            {
                return BadRequest("No se recibió ninguna factura.");
            }

            var results = new List<object>();
            foreach (var invoice in invoices)
            {
                var result = await _service.RegisterAsync(invoice);
                results.Add(new
                {
                    clientGuid = invoice.ClientGuid,
                    accepted = result.Invoice != null,
                    created = result.Created,
                    id = result.Invoice?.Id,
                    serie = result.Invoice?.Serie,
                    number = result.Invoice?.Number,
                    controlNumber = result.Invoice?.ControlNumber,
                    error = result.Error
                });
            }

            return Ok(results);
        }

        public class VoidRequest
        {
            public string Reason { get; set; } = string.Empty;
        }

        /// <summary>
        /// Marca la factura como anulada. Ojo: fiscalmente esto sólo cubre la anulación
        /// inmediata; para corregir una factura ya entregada al cliente hay que emitir
        /// una nota de crédito que la referencie (DocumentType = NotaCredito).
        /// </summary>
        [HttpPost("{id}/void")]
        public async Task<IActionResult> Void(int id, [FromBody] VoidRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Reason))
            {
                return BadRequest("La anulación debe indicar un motivo.");
            }

            var invoice = await _service.VoidAsync(id, request.Reason.Trim());
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }
    }
}
