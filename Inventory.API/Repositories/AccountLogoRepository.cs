using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class AccountLogoRepository : IAccountLogoRepository
    {
        private readonly AppDbContext _context;

        public AccountLogoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AccountLogo?> GetByAccountAsync(int accountId)
        {
            return await _context.AccountLogos
                .FirstOrDefaultAsync(l => l.CustomerAccountId == accountId);
        }

        /// <summary>
        /// Sólo los ids, sin traer los bytes: es lo que necesita la lista de cuentas.
        /// </summary>
        public async Task<HashSet<int>> GetAccountIdsWithLogoAsync()
        {
            var ids = await _context.AccountLogos
                .Select(l => l.CustomerAccountId)
                .ToListAsync();

            return ids.ToHashSet();
        }

        public async Task SaveAsync(AccountLogo logo)
        {
            var existing = await _context.AccountLogos
                .FirstOrDefaultAsync(l => l.CustomerAccountId == logo.CustomerAccountId);

            if (existing == null)
            {
                await _context.AccountLogos.AddAsync(logo);
            }
            else
            {
                existing.ContentType = logo.ContentType;
                existing.FileName = logo.FileName;
                existing.Data = logo.Data;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int accountId)
        {
            var existing = await _context.AccountLogos
                .FirstOrDefaultAsync(l => l.CustomerAccountId == accountId);

            if (existing != null)
            {
                _context.AccountLogos.Remove(existing);
                await _context.SaveChangesAsync();
            }
        }
    }
}
