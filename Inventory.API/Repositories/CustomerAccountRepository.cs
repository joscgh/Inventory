using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class CustomerAccountRepository : ICustomerAccountRepository
    {
        private readonly AppDbContext _context;

        public CustomerAccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerAccount>> GetAllAsync()
        {
            return await _context.CustomerAccounts
                .Include(a => a.Users)
                .ToListAsync();
        }

        public async Task<CustomerAccount?> GetByIdAsync(int id)
        {
            return await _context.CustomerAccounts
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<CustomerAccount?> GetByEmailAsync(string email)
        {
            return await _context.CustomerAccounts
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task AddAsync(CustomerAccount account)
        {
            await _context.CustomerAccounts.AddAsync(account);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerAccount account)
        {
            var existing = await _context.CustomerAccounts
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == account.Id);

            if (existing == null) return;

            _context.Entry(existing).CurrentValues.SetValues(account);
            existing.Users.Clear();
            existing.Users.AddRange(account.Users);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.CustomerAccounts.FindAsync(id);
            if (existing != null)
            {
                _context.CustomerAccounts.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
