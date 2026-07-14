using Inventory.API.Repositories;
using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _repository;
        private readonly IExchangeRateScraper _scraper;

        public CurrencyService(ICurrencyRepository repository, IExchangeRateScraper scraper)
        {
            _repository = repository;
            _scraper = scraper;
        }

        public async Task<IEnumerable<Currency>> ListCurrenciesAsync() =>
            await _repository.GetAllAsync();

        public async Task<Currency?> FindByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<bool> RegisterCurrencyAsync(Currency currency)
        {
            if (string.IsNullOrWhiteSpace(currency.Code) || string.IsNullOrWhiteSpace(currency.Name))
            {
                return false;
            }

            var existing = await _repository.GetByCodeAsync(currency.Code.Trim());
            if (existing != null) return false;

            currency.Code = currency.Code.Trim().ToUpperInvariant();
            currency.Name = currency.Name.Trim();
            currency.Symbol = currency.Symbol.Trim();

            await _repository.AddAsync(currency);
            return true;
        }

        public async Task<bool> ModifyCurrencyAsync(Currency currency)
        {
            var existing = await _repository.GetByIdAsync(currency.Id);
            if (existing == null) return false;

            existing.Code = currency.Code.Trim().ToUpperInvariant();
            existing.Name = currency.Name.Trim();
            existing.Symbol = currency.Symbol.Trim();

            await _repository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> RemoveCurrencyAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> RefreshExchangeRatesAsync()
        {
            var rates = await _scraper.ScrapeLatestRatesAsync();
            if (rates == null || !rates.Any())
            {
                return false;
            }

            var currencies = await _repository.GetAllAsync();
            var updated = false;

            foreach (var currency in currencies)
            {
                if (!rates.TryGetValue(currency.Code, out var latestRate) || latestRate == null)
                {
                    continue;
                }

                currency.ExchangeRate = latestRate;
                currency.LastUpdated = DateTime.UtcNow;
                await _repository.UpdateAsync(currency);
                updated = true;
            }

            return updated;
        }
    }
}
