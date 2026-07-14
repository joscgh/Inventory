using Inventory.API.Data;
using Inventory.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Inventory.API.Services
{
    public class BcvExchangeRateScraper : IExchangeRateScraper
    {
        private readonly HttpClient _http;

        public BcvExchangeRateScraper(HttpClient http)
        {
            _http = http;
        }

        public async Task<Dictionary<string, decimal?>> ScrapeLatestRatesAsync()
        {
            var result = new Dictionary<string, decimal?>();
            const string url = "https://www.bcv.org.ve/";
            string html;

            try
            {
                html = await _http.GetStringAsync(url);
            }
            catch
            {
                return result;
            }

            // Buscamos los contenedores de USD y EUR y extraemos el valor dentro de <strong>
            foreach (var currencyCode in new[] { "USD", "EUR" })
            {
                var currencyId = currencyCode == "USD" ? "dolar" : "euro";
                var pattern = $"<div id=\"{currencyId}\".*?<strong[^>]*>(.*?)</strong>";
                var match = Regex.Match(html, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (!match.Success)
                {
                    result[currencyCode] = null;
                    continue;
                }

                var valueText = match.Groups[1].Value;
                valueText = Regex.Replace(valueText, "[^0-9,.-]", string.Empty);
                valueText = valueText.Replace(',', '.');

                if (decimal.TryParse(valueText, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate))
                {
                    result[currencyCode] = rate;
                }
                else
                {
                    result[currencyCode] = null;
                }
            }

            return result;
        }
    }
}
