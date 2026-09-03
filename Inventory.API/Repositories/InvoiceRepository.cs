using Inventory.API.Data;
using Inventory.Core.Classes;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Invoice> WithGraph()
            => _context.Set<Invoice>()
                .Include(i => i.Terminal)
                .Include(i => i.CustomerAccount)
                .Include(i => i.Store)
                .Include(i => i.Warehouse)
                .Include(i => i.CreatedByUser)
                .Include(i => i.ConsumerCustomer)
                .Include(i => i.Currency)
                .Include(i => i.ReferenceInvoice)
                .Include(i => i.Lines)
                    .ThenInclude(l => l.Currency)
                .Include(i => i.Payments);

        public async Task<IEnumerable<Invoice>> GetAllAsync(
            InvoiceDocumentType? documentType = null,
            int? terminalId = null,
            int? customerAccountId = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            var query = WithGraph();

            if (documentType.HasValue) query = query.Where(i => i.DocumentType == documentType.Value);
            if (terminalId.HasValue) query = query.Where(i => i.TerminalId == terminalId.Value);
            if (customerAccountId.HasValue) query = query.Where(i => i.CustomerAccountId == customerAccountId.Value);
            if (from.HasValue) query = query.Where(i => i.IssuedAt >= from.Value);
            if (to.HasValue) query = query.Where(i => i.IssuedAt <= to.Value);

            return await query
                .OrderByDescending(i => i.IssuedAt)
                .ThenByDescending(i => i.Number)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByIdAsync(int id)
            => await WithGraph().FirstOrDefaultAsync(i => i.Id == id);

        public async Task<Invoice?> GetByClientGuidAsync(Guid clientGuid)
            => await WithGraph().FirstOrDefaultAsync(i => i.ClientGuid == clientGuid);

        public async Task<Invoice> AddAsync(Invoice invoice)
        {
            // La numeración y la inserción van juntas en una transacción: si algo falla
            // después de reservar el número, el número no se pierde ni se reutiliza mal.
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var range = invoice.Number > 0
                ? await LockRangeContainingAsync(invoice.TerminalId, invoice.DocumentType, invoice.Number)
                : await LockNextAvailableRangeAsync(invoice.TerminalId, invoice.DocumentType);

            if (range == null)
            {
                throw new InvalidOperationException(invoice.Number > 0
                    ? $"El número {invoice.Number} no pertenece a ningún rango asignado a esta caja."
                    : "La caja no tiene rangos de numeración disponibles. Asigna un bloque nuevo.");
            }

            if (invoice.Number <= 0)
            {
                invoice.Number = range.NextNumber;
            }

            // Avanzar el rango. Con facturas emitidas sin conexión pueden llegar
            // desordenadas, así que sólo se avanza hacia adelante, nunca hacia atrás.
            if (invoice.Number >= range.NextNumber)
            {
                range.NextNumber = invoice.Number + 1;
            }

            if (range.NextNumber > range.ToNumber)
            {
                range.Status = InvoiceRangeStatus.Exhausted;
            }

            invoice.InvoiceNumberRangeId = range.Id;
            invoice.Serie = string.IsNullOrWhiteSpace(invoice.Serie) ? range.Serie : invoice.Serie;

            if (string.IsNullOrWhiteSpace(invoice.ControlNumber))
            {
                invoice.ControlNumber = range.BuildControlNumber(invoice.Number);
            }

            invoice.ReceivedAtUtc = DateTime.UtcNow;
            if (invoice.IssuedAt == default)
            {
                invoice.IssuedAt = invoice.ReceivedAtUtc;
            }

            DetachNavigations(invoice);

            await _context.Set<Invoice>().AddAsync(invoice);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return invoice;
        }

        public async Task<Invoice?> VoidAsync(int id, string reason)
        {
            var invoice = await _context.Set<Invoice>().FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null) return null;
            if (invoice.Status == InvoiceStatus.Voided) return invoice;

            invoice.Status = InvoiceStatus.Voided;
            invoice.VoidedAtUtc = DateTime.UtcNow;
            invoice.VoidReason = reason;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        /// <summary>
        /// Bloquea (FOR UPDATE) el primer rango activo con números libres. El bloqueo es
        /// lo que impide que dos cajas pidiendo número a la vez reciban el mismo.
        /// </summary>
        private async Task<InvoiceNumberRange?> LockNextAvailableRangeAsync(int terminalId, InvoiceDocumentType documentType)
        {
            var ranges = await _context.Set<InvoiceNumberRange>()
                .FromSqlRaw(
                    """
                    SELECT * FROM "InvoiceNumberRanges"
                    WHERE "TerminalId" = {0}
                      AND "DocumentType" = {1}
                      AND "Status" = {2}
                      AND "NextNumber" <= "ToNumber"
                    ORDER BY "FromNumber"
                    LIMIT 1
                    FOR UPDATE
                    """,
                    terminalId, (int)documentType, (int)InvoiceRangeStatus.Active)
                .ToListAsync();

            return ranges.FirstOrDefault();
        }

        private async Task<InvoiceNumberRange?> LockRangeContainingAsync(int terminalId, InvoiceDocumentType documentType, long number)
        {
            var ranges = await _context.Set<InvoiceNumberRange>()
                .FromSqlRaw(
                    """
                    SELECT * FROM "InvoiceNumberRanges"
                    WHERE "TerminalId" = {0}
                      AND "DocumentType" = {1}
                      AND "Status" <> {2}
                      AND {3} BETWEEN "FromNumber" AND "ToNumber"
                    LIMIT 1
                    FOR UPDATE
                    """,
                    terminalId, (int)documentType, (int)InvoiceRangeStatus.Revoked, number)
                .ToListAsync();

            return ranges.FirstOrDefault();
        }

        /// <summary>
        /// Conserva las FK y anula las navegaciones. Sin esto EF intenta reinsertar
        /// monedas, ítems y clientes que ya existen. Mismo motivo que en NoteRepository.
        /// </summary>
        private static void DetachNavigations(Invoice invoice)
        {
            invoice.Terminal = null;
            invoice.NumberRange = null;
            invoice.CustomerAccount = null;
            invoice.Store = null;
            invoice.Warehouse = null;
            invoice.CreatedByUser = null;
            invoice.ConsumerCustomer = null;
            invoice.Currency = null;
            invoice.ReferenceInvoice = null;
            invoice.ReferencedByInvoices.Clear();

            foreach (var payment in invoice.Payments)
            {
                payment.Invoice = null;
                payment.Terminal = null;
            }

            foreach (var line in invoice.Lines)
            {
                line.Item = null;
                line.Category = null;
                line.Currency = null;
                line.Invoice = null;
            }
        }
    }
}
