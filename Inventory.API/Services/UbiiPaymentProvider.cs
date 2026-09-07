using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Inventory.Core.Services;
using Microsoft.Extensions.Options;

namespace Inventory.API.Services
{
    public sealed class UbiiPaymentProvider : IPaymentProvider
    {
        private readonly UbiiApiClient _client;
        private readonly UbiiOptions _options;

        public UbiiPaymentProvider(UbiiApiClient client, IOptions<UbiiOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public string Code => "ubii";

        public async Task<PaymentProviderResult> ChargeAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default)
        {
            if (request.MethodCode.Equals("mobile", StringComparison.OrdinalIgnoreCase))
            {
                return await _client.ValidateMobilePaymentAsync(request, cancellationToken);
            }

            if (request.MethodCode.Equals("card", StringComparison.OrdinalIgnoreCase))
            {
                return await _client.ProcessDebitCardAsync(request, cancellationToken);
            }

            return new PaymentProviderResult(false, ErrorMessage: "Ubii solo está configurado para Pago móvil y tarjeta de débito.");
        }

        public Task<PaymentProviderResult> CancelAsync(string reference, CancellationToken cancellationToken = default)
            => Task.FromResult(new PaymentProviderResult(false, reference, ErrorMessage: "Ubii no tiene reverso implementado para este flujo."));
    }
}