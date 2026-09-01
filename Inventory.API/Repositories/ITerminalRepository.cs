using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface ITerminalRepository
    {
        Task<IEnumerable<Terminal>> GetAllAsync(int? customerAccountId = null);
        Task<Terminal?> GetByIdAsync(int id);
        Task AddAsync(Terminal terminal);

        Task<IEnumerable<InvoiceNumberRange>> GetRangesAsync(int terminalId, InvoiceDocumentType? documentType = null);

        /// <summary>Último bloque entregado a la caja, para que el siguiente arranque donde terminó.</summary>
        Task<InvoiceNumberRange?> GetLastRangeAsync(int terminalId, InvoiceDocumentType documentType);

        Task AddRangeAsync(InvoiceNumberRange range);
    }
}
