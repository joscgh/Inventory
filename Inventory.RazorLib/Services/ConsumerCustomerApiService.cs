using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.RazorLib.Services
{
    public class ConsumerCustomerApiService
    {
        private readonly HttpClient _http;

        public ConsumerCustomerApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<(bool Success, ConsumerCustomer? Customer, string? ErrorMessage)> CreateConsumerCustomerAsync(ConsumerCustomer customer)
        {
            var response = await _http.PostAsJsonAsync("api/consumercustomers", customer);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<ConsumerCustomer>();
                return (true, created, null);
            }

            var errorText = await response.Content.ReadAsStringAsync();
            var message = string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText;
            return (false, null, message);
        }
    }
}
