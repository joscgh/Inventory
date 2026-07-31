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
            var items = await _context.Items
                .Include(i => i.Attributes)
                .Include(i => i.Currency)
                .Include(i => i.Category)
                .Include(i => i.Taxes)
                .Include(i => i.PriceVariants)
                .Include(i => i.StockByLocation)
                    .ThenInclude(s => s.Location)
                .ToListAsync();

            // Comprometido = entregado (reserva) menos acreditado (la nota de crédito
            // se emite contra una entrega y libera la reserva).
            var committedTotals = await _context.NoteLines
                .Where(nl => nl.ItemUniversalId.HasValue)
                .Join(
                    _context.Notes.Where(n => n.Type == NoteType.Entrega || n.Type == NoteType.Credito),
                    nl => nl.NoteId,
                    n => n.Id,
                    (nl, n) => new
                    {
                        ItemId = nl.ItemUniversalId!.Value,
                        Committed = n.Type == NoteType.Credito ? -nl.CommittedQuantity : nl.CommittedQuantity
                    })
                .GroupBy(x => x.ItemId)
                .Select(g => new { ItemId = g.Key, TotalCommitted = g.Sum(x => x.Committed) })
                .ToDictionaryAsync(g => g.ItemId, g => g.TotalCommitted);

            foreach (var item in items)
            {
                var total = committedTotals.TryGetValue(item.Id, out var value) ? value : 0m;
                item.CommittedQuantity = total < 0m ? 0m : total;
            }

            return items;
        }

        public async Task<ItemUniversal?> GetByIdAsync(string SKU)
        {
            return await _context.Items
                .Include(i => i.Attributes)
                .Include(i => i.Currency)
                .Include(i => i.Category)
                .Include(i => i.Taxes)
                .Include(i => i.PriceVariants)
                .Include(i => i.StockByLocation)
                    .ThenInclude(s => s.Location)
                .FirstOrDefaultAsync(i => i.SKU == SKU);
        }

        public async Task AddAsync(ItemUniversal item)
        {
            if (item.Taxes != null && item.Taxes.Any())
            {
                var taxIds = item.Taxes
                    .Where(t => t.Id != 0)
                    .Select(t => t.Id)
                    .ToList();

                item.Taxes = await _context.Taxes
                    .Where(t => taxIds.Contains(t.Id))
                    .ToListAsync();
            }

            await _context.Items.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemUniversal item)
        {
            // Para actualizar relaciones dinámicas limpiamente, removemos los anteriores y agregamos los nuevos
            var existingItem = await _context.Items
                .Include(i => i.Attributes)
                .Include(i => i.Taxes)
                .Include(i => i.PriceVariants)
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            if (existingItem != null)
            {
                _context.Entry(existingItem).CurrentValues.SetValues(item);
                existingItem.Attributes.Clear();
                existingItem.Attributes.AddRange(item.Attributes);

                existingItem.Taxes.Clear();
                if (item.Taxes != null && item.Taxes.Any())
                {
                    var taxIds = item.Taxes
                        .Where(t => t.Id != 0)
                        .Select(t => t.Id)
                        .ToList();

                    var taxes = await _context.Taxes
                        .Where(t => taxIds.Contains(t.Id))
                        .ToListAsync();

                    foreach (var tax in taxes)
                    {
                        existingItem.Taxes.Add(tax);
                    }
                }

                // Update price variants
                existingItem.PriceVariants.Clear();
                if (item.PriceVariants != null && item.PriceVariants.Any())
                {
                    foreach (var p in item.PriceVariants)
                    {
                        existingItem.PriceVariants.Add(new Core.Classes.PriceVariant
                        {
                            Label = p.Label,
                            Amount = p.Amount,
                            CurrencyId = p.CurrencyId
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStockAsync(ItemUniversal item)
        {
            // Ajuste de stock: solo tocamos el escalar Stock para NO alterar
            // relaciones dinámicas (PriceVariants, Taxes, Attributes).
            var existingItem = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == item.Id);

            if (existingItem != null)
            {
                existingItem.Stock = item.Stock;
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
