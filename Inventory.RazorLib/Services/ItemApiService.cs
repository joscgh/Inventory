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

        public async Task<decimal> GetTotalValueAsync() =>
            await _http.GetFromJsonAsync<decimal>("api/items/total-value");
    }
}