using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllAsync(
            InvoiceDocumentType? documentType = null,
            int? terminalId = null,
            int? customerAccountId = null,
            DateTime? from = null,
            DateTime? to = null);

        Task<Invoice?> GetByIdAsync(int id);

        /// <summary>Busca por el identificador que generó la caja, para no duplicar en los reintentos.</summary>
        Task<Invoice?> GetByClientGuidAsync(Guid clientGuid);

        /// <summary>
        /// Persiste la factura tomando su número del rango correspondiente, todo dentro
        /// de una transacción. Si la factura ya trae número (venía emitida sin conexión)
        /// se valida contra el rango en vez de asignar uno nuevo.
        /// </summary>
        Task<Invoice> AddAsync(Invoice invoice);

        Task<Invoice?> VoidAsync(int id, string reason);
    }
}
