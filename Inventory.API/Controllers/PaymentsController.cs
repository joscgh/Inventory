using Inventory.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public sealed class PaymentsController : ControllerBase
    {
        private readonly IPaymentProvider _provider;

        public PaymentsController(IPaymentProvider provider)
        {
            _provider = provider;
        }

        [HttpPost("charge")]
        public async Task<IActionResult> Charge(PaymentChargeRequest request, CancellationToken cancellationToken)
        {
            if (request.Amount <= 0m || request.TerminalId <= 0 || string.IsNullOrWhiteSpace(request.MethodCode))
            {
                return BadRequest("Monto, caja y método de pago son obligatorios.");
            }

            var result = await _provider.ChargeAsync(request, cancellationToken);
            return result.Approved ? Ok(result) : BadRequest(result);
        }

        [HttpPost("cancel/{reference}")]
        public async Task<IActionResult> Cancel(string reference, CancellationToken cancellationToken)
        {
            var result = await _provider.CancelAsync(reference, cancellationToken);
            return result.Approved ? Ok(result) : BadRequest(result);
        }
    }
}