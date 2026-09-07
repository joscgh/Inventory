using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace Inventory.SharedUI.Services
{
    public sealed class OfflineStorageService : IAsyncDisposable
    {
        private const string ModulePath = "/_content/Inventory.SharedUI/offlineStorage.js";
        private readonly IJSRuntime _js;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
        private IJSObjectReference? _module;

        public OfflineStorageService(IJSRuntime js) => _js = js;

        public async Task<bool> IsOnlineAsync() => await (await Module()).InvokeAsync<bool>("isOnline");

        public async Task<T?> ReadAsync<T>(string key)
        {
            var json = await (await Module()).InvokeAsync<string?>("getItem", key);
            return string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public async Task WriteAsync<T>(string key, T value)
            => await (await Module()).InvokeVoidAsync("setItem", key, JsonSerializer.Serialize(value, _jsonOptions));

        public async Task RemoveAsync(string key) => await (await Module()).InvokeVoidAsync("removeItem", key);

        private async Task<IJSObjectReference> Module()
            => _module ??= await _js.InvokeAsync<IJSObjectReference>("import", ModulePath);

        public async ValueTask DisposeAsync()
        {
            if (_module != null) await _module.DisposeAsync();
        }
    }
}