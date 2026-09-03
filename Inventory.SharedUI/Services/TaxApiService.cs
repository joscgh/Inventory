using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.SharedUI.Services
{
    public class TaxApiService
    {
        private readonly HttpClient _http;

        public TaxApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Tax>> GetTaxesAsync() =>
            await _http.GetFromJsonAsync<List<Tax>>("api/taxes") ?? new();

        public async Task<bool> SaveTaxAsync(Tax tax)
        {
            if (tax.Id == 0)
            {
                var response = await _http.PostAsJsonAsync("api/taxes", tax);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var response = await _http.PutAsJsonAsync($"api/taxes/{tax.Id}", tax);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> DeleteTaxAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/taxes/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
