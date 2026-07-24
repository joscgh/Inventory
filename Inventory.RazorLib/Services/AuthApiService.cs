using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.RazorLib.Services
{
    public class AuthApiService
    {
        private readonly HttpClient _http;

        public AuthApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", request);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task<(LoginResponse? Result, string? ErrorMessage)> RegisterAsync(RegisterAccountRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return (result, null);
            }

            var errorText = await response.Content.ReadAsStringAsync();
            return (null, string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText);
        }
    }
}
