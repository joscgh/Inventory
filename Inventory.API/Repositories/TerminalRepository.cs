using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class TerminalRepository : ITerminalRepository
    {
        private readonly AppDbContext _context;

        public TerminalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Terminal>> GetAllAsync(int? customerAccountId = null)
        {
            var query = _context.Set<Terminal>()
                .Include(t => t.Account)
                .Include(t => t.Store)
                .Include(t => t.Ranges)
                .AsQueryable();

            if (customerAccountId.HasValue)
            {
                query = query.Where(t => t.CustomerAccountId == customerAccountId.Value);
            }

            return await query.OrderBy(t => t.Code).ToListAsync();
        }

        public async Task<Terminal?> GetByIdAsync(int id)
            => await _context.Set<Terminal>()
                .Include(t => t.Account)
                .Include(t => t.Store)
                .Include(t => t.Ranges)
                .FirstOrDefaultAsync(t => t.Id == id);

        public async Task AddAsync(Terminal terminal)
        {
            terminal.Account = null;
            terminal.Store = null;
            terminal.CreatedAtUtc = DateTime.UtcNow;

            await _context.Set<Terminal>().AddAsync(terminal);
            await _context.SaveChangesAsync();
        }

        public Task<CustomerAccount?> GetAccountAsync(int id) =>
            _context.CustomerAccounts.FirstOrDefaultAsync(account => account.Id == id);

        public Task<AccountLocation?> GetStoreAsync(int id) =>
            _context.AccountLocations.FirstOrDefaultAsync(location => location.Id == id);

        public async Task<IEnumerable<TerminalPaymentMethod>> GetPaymentMethodsAsync(int terminalId) =>
            await _context.TerminalPaymentMethods
                .Where(method => method.TerminalId == terminalId)
                .OrderBy(method => method.Name)
                .ToListAsync();

        public async Task SavePaymentMethodsAsync(int terminalId, IEnumerable<TerminalPaymentMethod> methods)
        {
            var existing = await _context.TerminalPaymentMethods
                .Where(method => method.TerminalId == terminalId)
                .ToListAsync();
            _context.TerminalPaymentMethods.RemoveRange(existing);
            await _context.TerminalPaymentMethods.AddRangeAsync(methods.Select(method => new TerminalPaymentMethod
            {
                TerminalId = terminalId,
                Code = method.Code,
                Name = method.Name,
                IsActive = method.IsActive
            }));
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<InvoiceNumberRange>> GetRangesAsync(int terminalId, InvoiceDocumentType? documentType = null)
        {
            var query = _context.Set<InvoiceNumberRange>()
                .Where(r => r.TerminalId == terminalId);

            if (documentType.HasValue)
            {
                query = query.Where(r => r.DocumentType == documentType.Value);
            }

            return await query.OrderBy(r => r.FromNumber).ToListAsync();
        }

        public async Task<InvoiceNumberRange?> GetLastRangeAsync(int terminalId, InvoiceDocumentType documentType)
            => await _context.Set<InvoiceNumberRange>()
                .Where(r => r.TerminalId == terminalId && r.DocumentType == documentType)
                .OrderByDescending(r => r.ToNumber)
                .FirstOrDefaultAsync();

        public async Task AddRangeAsync(InvoiceNumberRange range)
        {
            range.Terminal = null;
            await _context.Set<InvoiceNumberRange>().AddAsync(range);
            await _context.SaveChangesAsync();
        }
    }
}
