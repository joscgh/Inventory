using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly AppDbContext _context;

        public NoteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            return await _context.Set<Note>()
                .Include(n => n.CustomerAccount)
                .Include(n => n.CreatedByUser)
                .Include(n => n.Currency)
                .Include(n => n.ReferenceNote)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Item)
                        .ThenInclude(i => i.Category)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Item)
                        .ThenInclude(i => i.Currency)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Currency)
                .OrderByDescending(n => n.IssueDate)
                .ToListAsync();
        }

        public async Task<Note?> GetByIdAsync(int id)
        {
            return await _context.Set<Note>()
                .Include(n => n.CustomerAccount)
                .Include(n => n.CreatedByUser)
                .Include(n => n.Currency)
                .Include(n => n.ReferenceNote)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Item)
                        .ThenInclude(i => i.Category)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Item)
                        .ThenInclude(i => i.Currency)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Currency)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<IEnumerable<Note>> GetByTypeAsync(NoteType type)
        {
            return await _context.Set<Note>()
                .Include(n => n.CustomerAccount)
                .Include(n => n.CreatedByUser)
                .Include(n => n.Currency)
                .Include(n => n.ReferenceNote)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Item)
                        .ThenInclude(i => i.Category)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Item)
                        .ThenInclude(i => i.Currency)
                .Include(n => n.Lines)
                    .ThenInclude(l => l.Currency)
                .Where(n => n.Type == type)
                .OrderByDescending(n => n.IssueDate)
                .ToListAsync();
        }

        public async Task AddAsync(Note note)
        {
            foreach (var line in note.Lines)
            {
                if (line.Item != null)
                {
                    line.ItemUniversalId = line.Item.Id;
                    line.CategoryId = line.Item.CategoryId;
                    line.CurrencyId ??= line.Item.CurrencyId;
                }
            }

            // En una nota de entrega se compromete (reserva) la cantidad entregada.
            // El stock físico no cambia; solo aumenta lo comprometido.
            if (note.Type == NoteType.Entrega)
            {
                foreach (var line in note.Lines)
                {
                    line.CommittedQuantity = line.Quantity;
                }
            }

            note.Subtotal = note.Lines.Sum(l => l.Subtotal);
            note.TotalTax = note.Lines.Sum(l => l.TaxAmount);
            note.Total = note.Lines.Sum(l => l.Total);

            if (!note.CurrencyId.HasValue)
            {
                note.CurrencyId = note.Lines
                    .Select(l => l.CurrencyId ?? l.Item?.CurrencyId)
                    .FirstOrDefault(id => id.HasValue);
            }

            if (!note.ExchangeRate.HasValue)
            {
                note.ExchangeRate = note.Lines
                    .Select(l => l.Currency?.ExchangeRate ?? l.Item?.Currency?.ExchangeRate ?? 1m)
                    .FirstOrDefault(rate => rate > 0m);
            }

            // Solo persistimos la nota y sus líneas. Las entidades relacionadas
            // (monedas, ítems, categorías, cliente, usuario, nota de referencia)
            // ya existen: conservamos sus FK y anulamos las navegaciones para que
            // EF no intente re-insertarlas (evita "duplicate key ... PK_Currencies").
            note.Currency = null;
            note.CustomerAccount = null;
            note.ConsumerCustomer = null;
            note.CreatedByUser = null;
            note.ReferenceNote = null;
            note.ReferencedByNotes.Clear();

            foreach (var line in note.Lines)
            {
                line.Item = null;
                line.Category = null;
                line.Currency = null;
                line.Note = null;
            }

            await _context.Set<Note>().AddAsync(note);
            await _context.SaveChangesAsync();

            // Reflejar la reserva de inventario en el historial (sin tocar el stock físico).
            if (note.Type == NoteType.Entrega)
            {
                await RecordCommitmentHistoryAsync(note);
            }
        }

        private async Task RecordCommitmentHistoryAsync(Note note)
        {
            var itemIds = note.Lines
                .Where(l => l.ItemUniversalId.HasValue && l.CommittedQuantity > 0)
                .Select(l => l.ItemUniversalId!.Value)
                .Distinct()
                .ToList();

            if (!itemIds.Any()) return;

            var items = await _context.Items
                .Where(i => itemIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => i);

            var userName = await _context.CustomerAccountUsers
                .Where(u => u.Id == note.CreatedByUserId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync();

            var adjustments = new List<InventoryAdjustment>();
            foreach (var line in note.Lines)
            {
                if (!line.ItemUniversalId.HasValue || line.CommittedQuantity <= 0) continue;
                if (!items.TryGetValue(line.ItemUniversalId.Value, out var item)) continue;

                adjustments.Add(new InventoryAdjustment
                {
                    ItemId = item.Id,
                    SKU = item.SKU,
                    Change = -(double)line.CommittedQuantity, // reduce la disponibilidad
                    PreviousStock = item.Stock,
                    NewStock = item.Stock,                    // el stock físico no cambia
                    Reason = "Comprometido por nota de entrega",
                    ReferenceType = "NotaEntrega",
                    ReferenceId = note.NoteNumber,
                    User = string.IsNullOrWhiteSpace(userName) ? null : userName,
                    Timestamp = DateTime.UtcNow
                });
            }

            if (adjustments.Any())
            {
                await _context.Adjustments.AddRangeAsync(adjustments);
                await _context.SaveChangesAsync();
            }
        }
    }
}
