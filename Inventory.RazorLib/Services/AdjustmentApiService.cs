using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.RazorLib.Services
{
    public class AdjustmentApiService
    {
        private readonly HttpClient _http;
        public AdjustmentApiService(HttpClient http) => _http = http;

        public async Task<List<InventoryAdjustment>> GetAdjustmentsAsync(string sku)
        {
            return await _http.GetFromJsonAsync<List<InventoryAdjustment>>($"api/items/{sku}/adjustments") ?? new List<InventoryAdjustment>();
        }

        public async Task<List<InventoryAdjustment>> GetAllAdjustmentsAsync()
        {
            return await _http.GetFromJsonAsync<List<InventoryAdjustment>>("api/inventory/history") ?? new List<InventoryAdjustment>();
        }

        public async Task<bool> CreateAdjustmentAsync(string sku, InventoryAdjustment adj)
        {
            var response = await _http.PostAsJsonAsync($"api/items/{sku}/adjustments", adj);
            return response.IsSuccessStatusCode;
        }
    }
}
