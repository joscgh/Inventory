using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.RazorLib.Services
{
    public class CurrencyApiService
    {
        private readonly HttpClient _http;

        public CurrencyApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Currency>> GetCurrenciesAsync() =>
            await _http.GetFromJsonAsync<List<Currency>>("api/currencies") ?? new();

        public async Task<bool> RefreshExchangeRatesAsync()
        {
            var response = await _http.PostAsync("api/currencies/refresh-rates", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SaveCurrencyAsync(Currency currency)
        {
            var response = await _http.PostAsJsonAsync("api/currencies", currency);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCurrencyAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/currencies/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
