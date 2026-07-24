using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class TaxRepository : ITaxRepository
    {
        private readonly AppDbContext _context;

        public TaxRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tax>> GetAllAsync() =>
            await _context.Taxes.ToListAsync();

        public async Task<Tax?> GetByIdAsync(int id) =>
            await _context.Taxes.FindAsync(id);

        public async Task<Tax?> GetByNameAsync(string name) =>
            await _context.Taxes.FirstOrDefaultAsync(t => t.Name == name);

        public async Task AddAsync(Tax tax)
        {
            await _context.Taxes.AddAsync(tax);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tax tax)
        {
            var existing = await _context.Taxes.FindAsync(tax.Id);
            if (existing == null) return;

            _context.Entry(existing).CurrentValues.SetValues(tax);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var tax = await _context.Taxes.FindAsync(id);
            if (tax == null) return;

            _context.Taxes.Remove(tax);
            await _context.SaveChangesAsync();
        }
    }
}
