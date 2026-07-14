using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> ListCurrenciesAsync();
        Task<Currency?> FindByIdAsync(int id);
        Task<bool> RegisterCurrencyAsync(Currency currency);
        Task<bool> ModifyCurrencyAsync(Currency currency);
        Task<bool> RemoveCurrencyAsync(int id);
        Task<bool> RefreshExchangeRatesAsync();
    }
}
