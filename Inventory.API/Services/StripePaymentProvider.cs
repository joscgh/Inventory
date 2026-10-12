using System.Net.Http.Headers;
using System.Net.Http.Json;
using Inventory.Core.Services;
using Microsoft.Extensions.Options;

namespace Inventory.API.Services
{
    public sealed class StripePaymentProvider : IPaymentProvider
    {
        private readonly HttpClient _http;
        private readonly StripeOptions _options;

        public StripePaymentProvider(HttpClient http, IOptions<StripeOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public string Code => _options.ProviderCode;

        public async Task<PaymentProviderResult> ChargeAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                return new PaymentProviderResult(false, ErrorMessage: "Stripe no está configurado para pagos internacionales.");
            }

            var payload = new Dictionary<string, object>
            {
                ["amount"] = (long)Math.Round(request.Amount * 100m),
                ["currency"] = request.CurrencyCode.ToLowerInvariant(),
                ["description"] = $"Invoice payment {request.TerminalId}",
                ["metadata"] = new Dictionary<string, string>
                {
                    ["terminalId"] = request.TerminalId.ToString(),
                    ["methodCode"] = request.MethodCode,
                    ["providerData"] = request.ProviderData ?? string.Empty
                }
            };

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, _options.ChargePath);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            requestMessage.Content = JsonContent.Create(payload);

            using var response = await _http.SendAsync(requestMessage, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new PaymentProviderResult(false, ErrorMessage: content ?? "Stripe rechazó la operación.");
            }

            var result = System.Text.Json.JsonDocument.Parse(content);
            var id = result.RootElement.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
            var status = result.RootElement.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null;
            var approved = string.Equals(status, "succeeded", StringComparison.OrdinalIgnoreCase);
            return new PaymentProviderResult(approved, id, id, approved ? null : "Stripe rechazó la operación internacional.");
        }

        public Task<PaymentProviderResult> CancelAsync(string reference, CancellationToken cancellationToken = default)
            => Task.FromResult(new PaymentProviderResult(false, reference, ErrorMessage: "Stripe no tiene reverso implementado en este flujo."));
    }
}
