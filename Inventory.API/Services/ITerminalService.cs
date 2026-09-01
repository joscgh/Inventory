using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    /// <summary>Datos para reservar un bloque de números a una caja.</summary>
    public class RangeAssignmentRequest
    {
        public InvoiceDocumentType DocumentType { get; set; } = InvoiceDocumentType.Factura;

        /// <summary>Cuántos números reservar.</summary>
        public int Size { get; set; } = 500;

        public string ControlPrefix { get; set; } = string.Empty;

        /// <summary>
        /// Número de control con el que arranca el bloque. Si viene nulo se continúa
        /// donde terminó el bloque anterior de esta caja.
        /// </summary>
        public long? ControlFromNumber { get; set; }

        public string? Authorization { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public interface ITerminalService
    {
        Task<IEnumerable<Terminal>> ListAsync(int? customerAccountId = null);
        Task<Terminal?> FindByIdAsync(int id);
        Task<(Terminal? Terminal, string? Error)> CreateAsync(Terminal terminal);
        Task<IEnumerable<InvoiceNumberRange>> ListRangesAsync(int terminalId, InvoiceDocumentType? documentType = null);
        Task<(InvoiceNumberRange? Range, string? Error)> AssignRangeAsync(int terminalId, RangeAssignmentRequest request);
    }
}
