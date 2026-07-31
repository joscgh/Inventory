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
                .Include(a => a.Locations)
                .ToListAsync();
        }

        public async Task<CustomerAccount?> GetByIdAsync(int id)
        {
            return await _context.CustomerAccounts
                .Include(a => a.Users)
                .Include(a => a.Locations)
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
                .FirstOrDefaultAsync(a => a.Id == account.Id);

            if (existing == null) return;

            // Sólo los datos propios de la cuenta. Los usuarios, depósitos y tiendas
            // se administran por sus propios endpoints, así que no se tocan aquí.
            existing.Name = account.Name;
            existing.Document = account.Document;
            existing.Address = account.Address;
            existing.Email = account.Email;
            existing.Phone = account.Phone;
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
