using System.Net.Http.Json;
using Inventory.Core.Services;

namespace Inventory.SharedUI.Services
{
    public sealed class PaymentApiService
    {
        private readonly HttpClient _http;

        public PaymentApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<PaymentProviderResult> ChargeAsync(PaymentChargeRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/payments/charge", request);
            var result = await response.Content.ReadFromJsonAsync<PaymentProviderResult>();
            return result ?? new PaymentProviderResult(false, ErrorMessage: "El servicio de pagos no devolvió respuesta.");
        }

        public async Task<PaymentProviderResult> CancelAsync(string reference)
        {
            var response = await _http.PostAsync($"api/payments/cancel/{Uri.EscapeDataString(reference)}", null);
            var result = await response.Content.ReadFromJsonAsync<PaymentProviderResult>();
            return result ?? new PaymentProviderResult(false, reference, ErrorMessage: "No se pudo confirmar el reverso.");
        }
    }
}