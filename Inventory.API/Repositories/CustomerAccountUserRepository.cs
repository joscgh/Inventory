using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class CustomerAccountUserRepository : ICustomerAccountUserRepository
    {
        private readonly AppDbContext _context;

        public CustomerAccountUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerAccountUser?> GetByEmailAsync(string email)
        {
            return await _context.CustomerAccountUsers
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<CustomerAccountUser?> GetByIdAsync(int id)
        {
            return await _context.CustomerAccountUsers
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task AddAsync(CustomerAccountUser user)
        {
            await _context.CustomerAccountUsers.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CustomerAccountUser user)
        {
            var existing = await _context.CustomerAccountUsers.FindAsync(user.Id);
            if (existing == null) return;

            _context.Entry(existing).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _context.CustomerAccountUsers.FindAsync(id);
            if (existing != null)
            {
                _context.CustomerAccountUsers.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
