using System.Text.Json;
using Inventory.Core.Classes;
using Microsoft.JSInterop;

namespace Inventory.SharedUI.Services
{
    public class UserStateService
    {
        private readonly IJSRuntime _jsRuntime;

        public UserStateService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<LoginResponse?> GetCurrentUserAsync()
        {
            var json = await _jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", "loggedUser");
            return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<LoginResponse>(json);
        }

        public async Task SetCurrentUserAsync(LoginResponse user)
        {
            var json = JsonSerializer.Serialize(user);
            await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "loggedUser", json);
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "loggedUser");
        }
    }
}
