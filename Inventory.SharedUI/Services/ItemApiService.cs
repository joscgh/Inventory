using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.SharedUI.Services
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

        public async Task<(bool Success, string? ErrorMessage)> AdjustStockAsync(
            string sku, double delta, string reason, string referenceType, int? locationId = null)
        {
            var request = new StockAdjustmentRequest
            {
                Delta = delta,
                Reason = reason,
                ReferenceType = referenceType,
                LocationId = locationId
            };
            var response = await _http.PostAsJsonAsync($"api/items/{sku}/adjust-stock", request);
            if (response.IsSuccessStatusCode) return (true, null);
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
        }

        /// <summary>Existencias del artículo desglosadas por depósito / tienda.</summary>
        public async Task<List<ItemStock>> GetStockByLocationAsync(string sku) =>
            await _http.GetFromJsonAsync<List<ItemStock>>($"api/items/{sku}/stock") ?? new();

        /// <summary>Fija la cantidad exacta del artículo en un depósito o tienda.</summary>
        public async Task<(bool Success, string? ErrorMessage)> SetStockAtLocationAsync(
            string sku, int locationId, double quantity, string reason)
        {
            var response = await _http.PutAsJsonAsync(
                $"api/items/{sku}/stock/{locationId}",
                new SetStockRequest { Quantity = quantity, Reason = reason });

            if (response.IsSuccessStatusCode) return (true, null);
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
        }

        private class StockAdjustmentRequest
        {
            public double Delta { get; set; }
            public string Reason { get; set; } = string.Empty;
            public string ReferenceType { get; set; } = string.Empty;
            public int? LocationId { get; set; }
        }

        private class SetStockRequest
        {
            public double Quantity { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        public async Task<decimal> GetTotalValueAsync() =>
            await _http.GetFromJsonAsync<decimal>("api/items/total-value");
    }
}