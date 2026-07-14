using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemUniversal>> GetAllAsync()
        {
            return await _context.Items
                .Include(i => i.Attributes)
                .Include(i => i.Currency)
                .Include(i => i.Category)
                .ToListAsync();
        }

        public async Task<ItemUniversal?> GetByIdAsync(string SKU)
        {
            return await _context.Items
                .Include(i => i.Attributes)
                .Include(i => i.Currency)
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.SKU == SKU);
        }

        public async Task AddAsync(ItemUniversal item)
        {
            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemUniversal item)
        {
            // Para actualizar relaciones dinámicas limpiamente, removemos los anteriores y agregamos los nuevos
            var existingItem = await _context.Items
                .Include(i => i.Attributes)
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            if (existingItem != null)
            {
                _context.Entry(existingItem).CurrentValues.SetValues(item);
                existingItem.Attributes.Clear();
                existingItem.Attributes.AddRange(item.Attributes);

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.SKU == id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
