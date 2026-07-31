using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class AdjustmentRepository : IAdjustmentRepository
    {
        private readonly AppDbContext _context;

        public AdjustmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(InventoryAdjustment adjustment)
        {
            await _context.Adjustments.AddAsync(adjustment);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InventoryAdjustment>> GetBySkuAsync(string sku)
        {
            return await _context.Adjustments
                .Where(a => a.SKU == sku)
                .Include(a => a.Item)
                    .ThenInclude(i => i.Category)
                .Include(a => a.Item)
                    .ThenInclude(i => i.Currency)
                .Include(a => a.Location)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<InventoryAdjustment>> GetAllAsync()
        {
            return await _context.Adjustments
                .Include(a => a.Item)
                    .ThenInclude(i => i.Category)
                .Include(a => a.Item)
                    .ThenInclude(i => i.Currency)
                .Include(a => a.Location)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}
