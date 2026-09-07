using System.Net.Http.Json;
using Inventory.Core.Services;
using Microsoft.Extensions.Options;

namespace Inventory.API.Services
{
    public sealed class GenericPosPaymentProvider : IPaymentProvider
    {
        private readonly HttpClient _http;
        private readonly GenericPosOptions _options;

        public GenericPosPaymentProvider(HttpClient http, IOptions<GenericPosOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public string Code => _options.ProviderCode;

        public async Task<PaymentProviderResult> ChargeAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                return new PaymentProviderResult(false, ErrorMessage: "El punto de venta no está configurado.");
            }

            var response = await _http.PostAsJsonAsync(_options.ChargePath, new
            {
                amount = request.Amount,
                currency = request.CurrencyCode,
                method = request.MethodCode,
                terminalId = request.TerminalId,
                providerData = request.ProviderData
            }, cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<GenericPosResponse>(cancellationToken: cancellationToken);
            if (result?.Approved == true)
            {
                return new PaymentProviderResult(true, result.Reference, result.AuthorizationCode);
            }

            return new PaymentProviderResult(false, result?.Reference, result?.AuthorizationCode,
                result?.Message ?? $"El punto de venta rechazó la operación ({(int)response.StatusCode}).");
        }

        public async Task<PaymentProviderResult> CancelAsync(string reference, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.BaseUrl))
            {
                return new PaymentProviderResult(false, ErrorMessage: "El punto de venta no está configurado.");
            }

            var response = await _http.PostAsJsonAsync(_options.CancelPath, new { reference }, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<GenericPosResponse>(cancellationToken: cancellationToken);
            return result?.Approved == true
                ? new PaymentProviderResult(true, result.Reference ?? reference, result.AuthorizationCode)
                : new PaymentProviderResult(false, reference, ErrorMessage: result?.Message ?? "No se pudo reversar el pago.");
        }

        private sealed class GenericPosResponse
        {
            public bool Approved { get; set; }
            public string? Reference { get; set; }
            public string? AuthorizationCode { get; set; }
            public string? Message { get; set; }
        }
    }
}