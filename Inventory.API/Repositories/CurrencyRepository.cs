using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly AppDbContext _context;

        public CurrencyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Currency>> GetAllAsync() =>
            await _context.Currencies.ToListAsync();

        public async Task<Currency?> GetByIdAsync(int id) =>
            await _context.Currencies.FindAsync(id);

        public async Task<Currency?> GetByCodeAsync(string code) =>
            await _context.Currencies.FirstOrDefaultAsync(c => c.Code == code);

        public async Task AddAsync(Currency currency)
        {
            await _context.Currencies.AddAsync(currency);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Currency currency)
        {
            var existing = await _context.Currencies.FindAsync(currency.Id);
            if (existing == null) return;

            _context.Entry(existing).CurrentValues.SetValues(currency);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency == null) return;

            _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync();
        }
    }
}
