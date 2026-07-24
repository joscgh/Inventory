using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.RazorLib.Services
{
    public class ItemApiService
    {
        private readonly HttpClient _http;

        public ItemApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ItemUniversal>> GetItemsAsync() =>
            await _http.GetFromJsonAsync<List<ItemUniversal>>("api/items") ?? new();

        public async Task<bool> SaveItemAsync(ItemUniversal item)
        {
            var response = await _http.PostAsJsonAsync("api/items", item);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var response = await _http.DeleteAsync($"api/items/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateItemAsync(ItemUniversal item)
        {
            var response = await _http.PutAsJsonAsync("api/items", item);
            return response.IsSuccessStatusCode;
        }

        public async Task<ItemUniversal?> GetItemBySkuAsync(string sku)
        {
            return await _http.GetFromJsonAsync<ItemUniversal>($"api/items/{sku}");
        }

        public async Task<bool> AdjustStockAsync(string sku, double delta, string reason, string referenceType)
        {
            var request = new StockAdjustmentRequest
            {
                Delta = delta,
                Reason = reason,
                ReferenceType = referenceType
            };
            var response = await _http.PostAsJsonAsync($"api/items/{sku}/adjust-stock", request);
            return response.IsSuccessStatusCode;
        }

        private class StockAdjustmentRequest
        {
            public double Delta { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string ReferenceType { get; set; } = string.Empty;
        }

        public async Task<decimal> GetTotalValueAsync() =>
            await _http.GetFromJsonAsync<decimal>("api/items/total-value");
    }
}