using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Inventory.Core.Services;
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
            using var response = await SendWithTransientRetryAsync(request, cancellationToken);
            await EnsureUbiiSuccessAsync(response, "check_client_id", cancellationToken);
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
            using var response = await SendWithTransientRetryAsync(request, cancellationToken);
            await EnsureUbiiSuccessAsync(response, "get_keys", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<UbiiKeysResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Ubii devolvió una respuesta vacía al obtener las llaves.");
            if (!string.Equals(result.R, "0", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(result.M ?? result.Ms ?? "Ubii no devolvió llaves para el comercio.");
            }

            return result;
        }

        public async Task<PaymentProviderResult> ValidateMobilePaymentAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default)
        {
            var data = ParseProviderData(request.ProviderData);
            var keys = await GetKeysAsync(cancellationToken);
            var apiKey = FindKey(keys.Keys, "P2C", "P2CR") ?? _options.PagoMovilApiKey;
            if (string.IsNullOrWhiteSpace(apiKey)) return new PaymentProviderResult(false, ErrorMessage: "Ubii no devolvió una llave P2C o P2CR para el comercio.");

            var payload = new Dictionary<string, object?>
            {
                ["bank"] = data.GetValueOrDefault("bank", string.Empty),
                ["date"] = data.GetValueOrDefault("date", DateTime.UtcNow.ToString("yyyyMMdd")),
                ["phoneP"] = data.GetValueOrDefault("phoneP", string.Empty),
                ["phoneC"] = data.GetValueOrDefault("phoneC", string.Empty),
                ["m"] = request.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                ["ref"] = data.GetValueOrDefault("ref", string.Empty),
                ["ci"] = data.GetValueOrDefault("ci", string.Empty),
                ["order"] = data.GetValueOrDefault("order", request.TerminalId.ToString())
            };

            return await SendPaymentAsync("payment_pago_movil_ref", apiKey, payload, cancellationToken);
        }

        public async Task<PaymentProviderResult> ProcessDebitCardAsync(PaymentChargeRequest request, CancellationToken cancellationToken = default)
        {
            var data = ParseProviderData(request.ProviderData);
            var keys = await GetKeysAsync(cancellationToken);
            var debitKey = keys.Keys.FirstOrDefault(key =>
                string.Equals(key.BtnAlias, "TDD", StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(_options.TarjetaDebitoBankCode)
                    || string.Equals(key.BtnBank, _options.TarjetaDebitoBankCode, StringComparison.OrdinalIgnoreCase)));
            var apiKey = debitKey?.BtnKey ?? _options.TarjetaDebitoApiKey;
            if (string.IsNullOrWhiteSpace(apiKey)) return new PaymentProviderResult(false, ErrorMessage: "Ubii no devolvió una llave TDD para el comercio.");

            var payload = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in data)
            {
                payload[item.Key] = item.Value;
            }

            payload["m"] = request.Amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            payload["cu"] = request.CurrencyCode;
            payload["order"] = data.GetValueOrDefault("order", request.TerminalId.ToString());
            return await SendPaymentAsync(_options.TarjetaDebitoPath, apiKey, payload, cancellationToken);
        }

        private static string? FindKey(IEnumerable<UbiiPaymentKey> keys, params string[] aliases)
        {
            var aliasSet = aliases
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return keys
                .Where(key => !string.IsNullOrWhiteSpace(key.BtnKey)
                              && aliasSet.Contains(key.BtnAlias ?? string.Empty))
                .Select(key => key.BtnKey)
                .FirstOrDefault();
        }

        private async Task<PaymentProviderResult> SendPaymentAsync(string path, string apiKey, Dictionary<string, object?> payload, CancellationToken cancellationToken)
        {
            var check = await CheckClientAsync(cancellationToken);
            var claims = ReadTokenClaims(check.Token!);
            var encrypted = Encrypt(JsonSerializer.Serialize(payload), claims.Key, claims.Iv);
            using var request = new HttpRequestMessage(HttpMethod.Post, path);
            AddCommonHeaders(request);
            request.Headers.Add("X-API-KEY", apiKey);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", check.Token);
            request.Content = JsonContent.Create(encrypted);
            using var response = await SendWithTransientRetryAsync(request, cancellationToken);
            await EnsureUbiiSuccessAsync(response, path, cancellationToken);
            var body = await response.Content.ReadFromJsonAsync<UbiiPaymentResponse>(cancellationToken: cancellationToken);
            var approved = body != null && body.R == "0" && body.M?.Contains("APROB", StringComparison.OrdinalIgnoreCase) == true;
            return new PaymentProviderResult(approved, body?.Ref, body?.Trace, approved ? null : body?.CodS ?? body?.M ?? "Ubii rechazó el pago.");
        }

        private async Task<HttpResponseMessage> SendWithTransientRetryAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await _http.SendAsync(request, cancellationToken);
            if (response.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable)
            {
                return response;
            }

            response.Dispose();
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            return await _http.SendAsync(await CloneRequestAsync(request, cancellationToken), cancellationToken);
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content is not null)
            {
                var content = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                clone.Content = new ByteArrayContent(content);
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }

        private static async Task EnsureUbiiSuccessAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            var suffix = string.IsNullOrWhiteSpace(detail) ? string.Empty : $" Respuesta: {detail.Trim()}";
            throw new HttpRequestException(
                $"Ubii no está disponible para {operation}. HTTP {(int)response.StatusCode} ({response.ReasonPhrase}). URL: {response.RequestMessage?.RequestUri}.{suffix}",
                null,
                response.StatusCode);
        }

        private static Dictionary<string, string> ParseProviderData(string? providerData)
        {
            if (string.IsNullOrWhiteSpace(providerData))
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                using var document = JsonDocument.Parse(providerData);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    var key = property.Name.TrimStart('@');
                    result[key] = property.Value.ToString();
                }

                return result;
            }
            catch (JsonException)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static (byte[] Key, byte[] Iv) ReadTokenClaims(string token)
        {
            var parts = token.Split('.');
            var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            return (Encoding.UTF8.GetBytes(root.GetProperty("k").GetString()!), Encoding.UTF8.GetBytes(root.GetProperty("i").GetString()!));
        }

        private static string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using var encryptor = aes.CreateEncryptor();
            var bytes = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(plainText), 0, Encoding.UTF8.GetByteCount(plainText));
            return Convert.ToBase64String(bytes);
        }

        private static byte[] Base64UrlDecode(string value)
        {
            value = value.Replace('-', '+').Replace('_', '/');
            value = value.PadRight(value.Length + (4 - value.Length % 4) % 4, '=');
            return Convert.FromBase64String(value);
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

    public sealed class UbiiPaymentResponse
    {
        public string? R { get; set; }
        public string? M { get; set; }
        public string? Ref { get; set; }
        public string? CodR { get; set; }
        public string? CodS { get; set; }
        public string? Trace { get; set; }
    }
}