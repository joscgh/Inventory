using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumerCustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConsumerCustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ConsumerCustomer consumerCustomer)
        {
            if (consumerCustomer == null)
            {
                return BadRequest("Cliente consumidor inválido.");
            }

            if (string.IsNullOrWhiteSpace(consumerCustomer.Name) && string.IsNullOrWhiteSpace(consumerCustomer.Document))
            {
                return BadRequest("El cliente consumidor debe incluir nombre o documento.");
            }

            var normalizedDocument = consumerCustomer.Document?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(normalizedDocument))
            {
                var existing = await _context.ConsumerCustomers
                    .FirstOrDefaultAsync(c => c.Document.ToLower() == normalizedDocument.ToLower());

                if (existing != null)
                {
                    return Ok(existing);
                }
            }

            consumerCustomer.Name = consumerCustomer.Name?.Trim() ?? string.Empty;
            consumerCustomer.Document = normalizedDocument;
            consumerCustomer.Address = consumerCustomer.Address?.Trim() ?? string.Empty;
            consumerCustomer.Email = consumerCustomer.Email?.Trim() ?? string.Empty;
            consumerCustomer.Phone = consumerCustomer.Phone?.Trim() ?? string.Empty;

            _context.ConsumerCustomers.Add(consumerCustomer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new { id = consumerCustomer.Id }, consumerCustomer);
        }
    }
}
