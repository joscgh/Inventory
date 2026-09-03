using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class AccountLocationRepository : IAccountLocationRepository
    {
        private readonly AppDbContext _context;

        public AccountLocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AccountLocation>> GetByAccountAsync(int accountId)
        {
            return await _context.AccountLocations
                .Where(l => l.CustomerAccountId == accountId)
                .OrderBy(l => l.Type)
                .ThenBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<AccountLocation?> GetByIdAsync(int id)
        {
            return await _context.AccountLocations
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<AccountLocation?> GetByNameAsync(int accountId, string name)
        {
            return await _context.AccountLocations
                .FirstOrDefaultAsync(l => l.CustomerAccountId == accountId && l.Name == name);
        }

        public async Task AddAsync(AccountLocation location)
        {
            await _context.AccountLocations.AddAsync(location);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AccountLocation location)
        {
            var existing = await _context.AccountLocations
                .FirstOrDefaultAsync(l => l.Id == location.Id);

            if (existing == null) return;

            existing.Name = location.Name;
            existing.Type = location.Type;
            existing.Address = location.Address;
            existing.Rif = location.Rif;
            existing.Phone = location.Phone;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.AccountLocations.FindAsync(id);
            if (existing != null)
            {
                _context.AccountLocations.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
