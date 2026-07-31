using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class ItemStockRepository : IItemStockRepository
    {
        private readonly AppDbContext _context;

        public ItemStockRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemStock>> GetByItemAsync(int itemId)
        {
            return await _context.ItemStocks
                .Include(s => s.Location)
                .Where(s => s.ItemId == itemId)
                .OrderBy(s => s.Location!.Type)
                .ThenBy(s => s.Location!.Name)
                .ToListAsync();
        }

        public async Task<List<int>> GetLocationIdsWithStockAsync(int itemId)
        {
            return await _context.ItemStocks
                .Where(s => s.ItemId == itemId && s.Quantity != 0)
                .Select(s => s.LocationId)
                .ToListAsync();
        }

        public async Task<double> SetQuantityAsync(int itemId, int locationId, double quantity)
        {
            if (quantity < 0) quantity = 0;
            quantity = Math.Round(quantity, 2);

            var row = await _context.ItemStocks
                .FirstOrDefaultAsync(s => s.ItemId == itemId && s.LocationId == locationId);

            if (row == null)
            {
                await _context.ItemStocks.AddAsync(new ItemStock
                {
                    ItemId = itemId,
                    LocationId = locationId,
                    Quantity = quantity
                });
            }
            else
            {
                row.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            return await RecomputeItemTotalAsync(itemId);
        }

        public async Task<(double PreviousAtLocation, double NewAtLocation, double NewTotal)> AdjustAsync(
            int itemId, int locationId, double delta)
        {
            var row = await _context.ItemStocks
                .FirstOrDefaultAsync(s => s.ItemId == itemId && s.LocationId == locationId);

            var previous = row?.Quantity ?? 0;
            var updated = Math.Round(Math.Max(0, previous + delta), 2);

            if (row == null)
            {
                await _context.ItemStocks.AddAsync(new ItemStock
                {
                    ItemId = itemId,
                    LocationId = locationId,
                    Quantity = updated
                });
            }
            else
            {
                row.Quantity = updated;
            }

            await _context.SaveChangesAsync();
            var total = await RecomputeItemTotalAsync(itemId);
            return (previous, updated, total);
        }

        /// <summary>
        /// ItemUniversal.Stock es el total cacheado: se recalcula desde las
        /// existencias por ubicación cada vez que una de ellas cambia.
        /// </summary>
        private async Task<double> RecomputeItemTotalAsync(int itemId)
        {
            var total = await _context.ItemStocks
                .Where(s => s.ItemId == itemId)
                .SumAsync(s => s.Quantity);

            total = Math.Round(total, 2);

            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item != null && Math.Abs(item.Stock - total) > 0.0001)
            {
                item.Stock = total;
                await _context.SaveChangesAsync();
            }

            return total;
        }
    }
}
