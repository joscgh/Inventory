using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    /// <summary>
    /// Resultado de registrar una factura. <paramref name="Created"/> distingue una
    /// emisión real de un reintento de sincronización que ya estaba registrado: el POS
    /// necesita saber la diferencia para vaciar su cola sin volver a imprimir.
    /// </summary>
    public record InvoiceRegistrationResult(Invoice? Invoice, bool Created, string? Error)
    {
        public static InvoiceRegistrationResult Fail(string error) => new(null, false, error);
        public static InvoiceRegistrationResult Issued(Invoice invoice) => new(invoice, true, null);
        public static InvoiceRegistrationResult Duplicate(Invoice invoice) => new(invoice, false, null);
    }

    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> ListAsync(
            InvoiceDocumentType? documentType = null,
            int? terminalId = null,
            int? customerAccountId = null,
            DateTime? from = null,
            DateTime? to = null);

        Task<Invoice?> FindByIdAsync(int id);

        Task<InvoiceRegistrationResult> RegisterAsync(Invoice invoice);

        Task<Invoice?> VoidAsync(int id, string reason);
    }
}
