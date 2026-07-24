using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.RazorLib.Services
{
    public class CustomerAccountApiService
    {
        private readonly HttpClient _http;

        public CustomerAccountApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CustomerAccount>> GetAccountsAsync() =>
            await _http.GetFromJsonAsync<List<CustomerAccount>>("api/customeraccounts") ?? new();

        public async Task<CustomerAccount?> GetAccountByIdAsync(int id) =>
            await _http.GetFromJsonAsync<CustomerAccount>($"api/customeraccounts/{id}");

        public async Task<(bool Success, string? ErrorMessage)> SaveAccountAsync(CustomerAccount account)
        {
            var response = await _http.PostAsJsonAsync("api/customeraccounts", account);
            if (response.IsSuccessStatusCode) return (true, null);
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
        }

        public async Task<(bool Success, CustomerAccount? Account, string? ErrorMessage)> CreateAccountAsync(CustomerAccount account)
        {
            var response = await _http.PostAsJsonAsync("api/customeraccounts", account);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                return (false, null, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
            }

            var createdAccount = await response.Content.ReadFromJsonAsync<CustomerAccount>();
            return (true, createdAccount, null);
        }

        public async Task<bool> UpdateAccountAsync(CustomerAccount account)
        {
            var response = await _http.PutAsJsonAsync("api/customeraccounts", account);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? ErrorMessage)> AddUserAsync(int accountId, AddAccountUserRequest request)
        {
            var response = await _http.PostAsJsonAsync($"api/customeraccounts/{accountId}/users", request);
            if (response.IsSuccessStatusCode) return (true, null);
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
        }

        public async Task<bool> DeleteAccountAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/customeraccounts/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
