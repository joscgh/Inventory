using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Inventory.API.Services
{
    public sealed class UbiiApiClient
    {
        private readonly HttpClient _http;
        private readonly UbiiOptions _options;
        private string? _token;
        private DateTimeOffset _tokenExpiresAt;

        public UbiiApiClient(HttpClient http, IOptions<UbiiOptions> options)
        {
            _http = http;
            _options = options.Value;
        }

        public async Task<UbiiClientCheckResponse> CheckClientAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            using var request = new HttpRequestMessage(HttpMethod.Get, "check_client_id");
            AddCommonHeaders(request);
            using var response = await _http.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UbiiClientCheckResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Ubii devolvió una respuesta vacía al validar el comercio.");
            if (!string.Equals(result.R, "0", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(result.Token))
            {
                throw new InvalidOperationException(result.M ?? result.Ms ?? "Ubii no autorizó el comercio.");
            }

            _token = result.Token;
            _tokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(14);
            return result;
        }

        public async Task<UbiiKeysResponse> GetKeysAsync(CancellationToken cancellationToken = default)
        {
            ValidateConfiguration();
            if (string.IsNullOrWhiteSpace(_token) || DateTimeOffset.UtcNow >= _tokenExpiresAt)
            {
                await CheckClientAsync(cancellationToken);
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, "get_keys");
            AddCommonHeaders(request);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            using var response = await _http.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<UbiiKeysResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Ubii devolvió una respuesta vacía al obtener las llaves.");
            if (!string.Equals(result.R, "0", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(result.M ?? result.Ms ?? "Ubii no devolvió llaves para el comercio.");
            }

            return result;
        }

        private void AddCommonHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("X-CLIENT-ID", _options.ClientId);
            request.Headers.Add("X-CLIENT-DOMAIN", _options.ClientDomain);
            request.Headers.Add("X-CLIENT-CHANNEL", _options.Channel);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void ValidateConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientDomain))
            {
                throw new InvalidOperationException("Ubii no está configurado: faltan ClientId o ClientDomain.");
            }
        }
    }

    public sealed class UbiiClientCheckResponse
    {
        public string? R { get; set; }
        public string? M { get; set; }
        public string? Ms { get; set; }
        public string? Token { get; set; }
        public int? IdComercio { get; set; }
        public string? RifComercio { get; set; }
        public string? NameComercio { get; set; }
    }

    public sealed class UbiiKeysResponse
    {
        public string? R { get; set; }
        public string? M { get; set; }
        public string? Ms { get; set; }
        public List<UbiiPaymentKey> Keys { get; set; } = new();
    }

    public sealed class UbiiPaymentKey
    {
        public string? BtnAlias { get; set; }
        public string? BtnKey { get; set; }
        public string? BtnName { get; set; }
        public string? BtnBank { get; set; }
    }
}