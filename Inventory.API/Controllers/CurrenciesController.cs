using Inventory.API.Services;
using Inventory.Core.Classes;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrenciesController : ControllerBase
    {
        private readonly ICurrencyService _service;

        public CurrenciesController(ICurrencyService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Currency>>> Get()
        {
            var currencies = await _service.ListCurrenciesAsync();
            return Ok(currencies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Currency>> GetById(int id)
        {
            var currency = await _service.FindByIdAsync(id);
            if (currency == null) return NotFound();
            return Ok(currency);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Currency currency)
        {
            var created = await _service.RegisterCurrencyAsync(currency);
            if (!created) return BadRequest("Unable to save currency. It may already exist or have invalid data.");
            return CreatedAtAction(nameof(GetById), new { id = currency.Id }, currency);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Currency currency)
        {
            if (id != currency.Id) return BadRequest("Currency ID mismatch.");

            var updated = await _service.ModifyCurrencyAsync(currency);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var removed = await _service.RemoveCurrencyAsync(id);
            if (!removed) return NotFound();
            return NoContent();
        }

        [HttpPost("refresh-rates")]
        public async Task<IActionResult> RefreshRates()
        {
            var refreshed = await _service.RefreshExchangeRatesAsync();
            if (!refreshed)
            {
                return StatusCode(503, "Unable to refresh exchange rates from BCV.");
            }

            return NoContent();
        }
    }
}
