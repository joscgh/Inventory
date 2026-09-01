using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.SharedUI.Services
{
    public class CategoryApiService
    {
        private readonly HttpClient _http;

        public CategoryApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Category>> GetCategoriesAsync() =>
            await _http.GetFromJsonAsync<List<Category>>("api/categories") ?? new();

        public async Task<Category?> GetCategoryByIdAsync(int id) =>
            await _http.GetFromJsonAsync<Category>($"api/categories/{id}");

        public async Task<bool> SaveCategoryAsync(Category category)
        {
            if (category.Id == 0)
            {
                var response = await _http.PostAsJsonAsync("api/categories", category);
                return response.IsSuccessStatusCode;
            }
            else
            {
                var response = await _http.PutAsJsonAsync($"api/categories/{category.Id}", category);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/categories/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
