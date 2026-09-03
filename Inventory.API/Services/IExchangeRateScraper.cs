using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventory.API.Services
{
    public interface IExchangeRateScraper
    {
        Task<Dictionary<string, decimal?>> ScrapeLatestRatesAsync();
    }
}
