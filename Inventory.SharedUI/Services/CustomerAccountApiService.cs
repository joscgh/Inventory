using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.SharedUI.Services
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

        /// <summary>Tamaño máximo del logo aceptado por el API, en bytes.</summary>
        public const long MaxLogoSizeBytes = 2 * 1024 * 1024;

        /// <summary>
        /// URL absoluta del logo, para usar directamente en un &lt;img src&gt;.
        /// El parámetro v evita que el navegador muestre en caché un logo ya reemplazado.
        /// </summary>
        public string GetLogoUrl(int accountId, int version = 0) =>
            new Uri(_http.BaseAddress!, $"api/customeraccounts/{accountId}/logo?v={version}").ToString();

        public async Task<(bool Success, string? ErrorMessage)> UploadLogoAsync(
            int accountId, Stream content, string fileName)
        {
            using var form = new MultipartFormDataContent();
            var fileContent = new StreamContent(content);
            form.Add(fileContent, "file", fileName);

            var response = await _http.PostAsync($"api/customeraccounts/{accountId}/logo", form);
            if (response.IsSuccessStatusCode) return (true, null);
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
        }

        public async Task<bool> DeleteLogoAsync(int accountId)
        {
            var response = await _http.DeleteAsync($"api/customeraccounts/{accountId}/logo");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<AccountLocation>> GetLocationsAsync(int accountId) =>
            await _http.GetFromJsonAsync<List<AccountLocation>>($"api/customeraccounts/{accountId}/locations") ?? new();

        public async Task<(bool Success, string? ErrorMessage)> AddLocationAsync(int accountId, AccountLocation location)
        {
            var response = await _http.PostAsJsonAsync($"api/customeraccounts/{accountId}/locations", location);
            if (response.IsSuccessStatusCode) return (true, null);
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateLocationAsync(int accountId, AccountLocation location)
        {
            var response = await _http.PutAsJsonAsync($"api/customeraccounts/{accountId}/locations/{location.Id}", location);
            if (response.IsSuccessStatusCode) return (true, null);
            var errorText = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
        }

        public async Task<bool> DeleteLocationAsync(int accountId, int locationId)
        {
            var response = await _http.DeleteAsync($"api/customeraccounts/{accountId}/locations/{locationId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAccountAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/customeraccounts/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
