using Inventory.API.Repositories;
using Inventory.Core.Classes;
using Inventory.Core.Services;

namespace Inventory.API.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _repository;
        private readonly ITerminalRepository _terminals;

        public InvoiceService(IInvoiceRepository repository, ITerminalRepository terminals)
        {
            _repository = repository;
            _terminals = terminals;
        }

        public Task<IEnumerable<Invoice>> ListAsync(
            InvoiceDocumentType? documentType = null,
            int? terminalId = null,
            int? customerAccountId = null,
            DateTime? from = null,
            DateTime? to = null)
            => _repository.GetAllAsync(documentType, terminalId, customerAccountId, from, to);

        public Task<Invoice?> FindByIdAsync(int id) => _repository.GetByIdAsync(id);

        public async Task<InvoiceRegistrationResult> RegisterAsync(Invoice invoice)
        {
            if (invoice.ClientGuid == Guid.Empty)
            {
                return InvoiceRegistrationResult.Fail(
                    "La factura debe traer ClientGuid: es lo que evita duplicados al sincronizar.");
            }

            if (invoice.Lines == null || invoice.Lines.Count == 0)
            {
                return InvoiceRegistrationResult.Fail("La factura debe incluir al menos una línea.");
            }

            if (invoice.CreatedByUserId <= 0)
            {
                return InvoiceRegistrationResult.Fail("La factura debe registrar el usuario que la emitió.");
            }

            if (string.IsNullOrWhiteSpace(invoice.CustomerName))
            {
                return InvoiceRegistrationResult.Fail("La factura debe identificar al cliente.");
            }

            if (invoice.Lines.Any(line => line.Quantity <= 0m
                || line.UnitPrice < 0m
                || line.Discount < 0m
                || line.Discount > line.UnitPrice * line.Quantity
                || line.TaxRate < 0m
                || line.TaxRate > 100m))
            {
                return InvoiceRegistrationResult.Fail("La factura contiene una cantidad, descuento, precio o impuesto inválido.");
            }

            var emissionError = ValidateEmissionMode(invoice);
            if (emissionError != null)
            {
                return InvoiceRegistrationResult.Fail(emissionError);
            }

            // Reintento de una factura que ya subió: se devuelve la que está guardada,
            // sin tocar la numeración. El POS puede reenviar sin miedo.
            var existing = await _repository.GetByClientGuidAsync(invoice.ClientGuid);
            if (existing != null)
            {
                return InvoiceRegistrationResult.Duplicate(existing);
            }

            var terminal = await _terminals.GetByIdAsync(invoice.TerminalId);
            if (terminal == null)
            {
                return InvoiceRegistrationResult.Fail("La caja indicada no existe.");
            }

            if (invoice.CustomerAccountId > 0 && invoice.CustomerAccountId != terminal.CustomerAccountId)
            {
                return InvoiceRegistrationResult.Fail("La caja seleccionada no pertenece a la cuenta del usuario.");
            }

            if (!terminal.IsActive)
            {
                return InvoiceRegistrationResult.Fail($"La caja {terminal.Code} está desactivada y no puede facturar.");
            }

            if (terminal.Store == null
                || string.IsNullOrWhiteSpace(terminal.Store.Address)
                || !FiscalIdentifierValidator.IsValidRif(terminal.Store.Rif))
            {
                return InvoiceRegistrationResult.Fail(
                    "La caja debe tener una tienda emisora configurada con dirección y RIF válido.");
            }

            if (terminal.Account == null
                || !FiscalIdentifierValidator.IsValidRif(terminal.Account.Document)
                || terminal.Store.CustomerAccountId != terminal.CustomerAccountId
                || !string.Equals(
                    FiscalIdentifierValidator.NormalizeRif(terminal.Account.Document),
                    FiscalIdentifierValidator.NormalizeRif(terminal.Store.Rif),
                    StringComparison.OrdinalIgnoreCase))
            {
                return InvoiceRegistrationResult.Fail(
                    "La tienda emisora debe pertenecer al mismo contribuyente que la cuenta de la caja.");
            }

            if (invoice.CustomerAccountId <= 0)
            {
                invoice.CustomerAccountId = terminal.CustomerAccountId;
            }

            invoice.StoreId ??= terminal.StoreId;
            invoice.Serie = string.IsNullOrWhiteSpace(invoice.Serie) ? terminal.Serie : invoice.Serie;

            ApplyTotals(invoice);

            var paymentError = ValidatePayments(invoice, terminal);
            if (paymentError != null)
            {
                return InvoiceRegistrationResult.Fail(paymentError);
            }

            try
            {
                var saved = await _repository.AddAsync(invoice);
                return InvoiceRegistrationResult.Issued(saved);
            }
            catch (InvalidOperationException ex)
            {
                // Rango agotado o número fuera de rango: es un error de negocio, no un 500.
                return InvoiceRegistrationResult.Fail(ex.Message);
            }
        }

        public Task<Invoice?> VoidAsync(int id, string reason) => _repository.VoidAsync(id, reason);

        private static string? ValidateEmissionMode(Invoice invoice)
        {
            return invoice.EmissionMode switch
            {
                InvoiceEmissionMode.Digital when string.IsNullOrWhiteSpace(invoice.FiscalDocumentId)
                    => "La facturación digital requiere el identificador emitido por un proveedor autorizado.",
                InvoiceEmissionMode.MaquinaFiscal when string.IsNullOrWhiteSpace(invoice.FiscalDeviceSerial)
                    => "La máquina fiscal requiere el serial del equipo autorizado.",
                InvoiceEmissionMode.MaquinaFiscal when string.IsNullOrWhiteSpace(invoice.FiscalDocumentId)
                    => "La máquina fiscal requiere el número devuelto por el equipo fiscal.",
                _ => null
            };
        }

        private static string? ValidatePayments(Invoice invoice, Terminal terminal)
        {
            if (invoice.Payments == null || invoice.Payments.Count == 0)
            {
                return "La factura debe registrar al menos un método de pago.";
            }

            if (invoice.Payments.Any(payment => payment.TerminalId != terminal.Id
                || payment.Amount <= 0m
                || payment.Status != PaymentStatus.Approved
                || !string.Equals(payment.CurrencyCode, "VES", StringComparison.OrdinalIgnoreCase)))
            {
                return "El pago debe estar aprobado, expresado en VES y asociado a la caja seleccionada.";
            }

            if (invoice.Payments.Sum(payment => payment.Amount) < invoice.Total)
            {
                return "Los pagos registrados no cubren el total de la factura.";
            }

            return null;
        }

        /// <summary>
        /// Los importes se recalculan en el servidor a partir de las líneas: lo que
        /// mande la caja en las cabeceras es informativo. Con facturación offline el
        /// cliente no es una fuente confiable de totales.
        /// </summary>
        private static void ApplyTotals(Invoice invoice)
        {
            foreach (var line in invoice.Lines)
            {
                if (line.Item != null)
                {
                    line.ItemUniversalId ??= line.Item.Id;
                    line.CategoryId ??= line.Item.CategoryId;
                    line.CurrencyId ??= line.Item.CurrencyId;

                    if (string.IsNullOrWhiteSpace(line.Description))
                    {
                        line.Description = line.Item.Name;
                    }
                }

                line.ExchangeRate ??= invoice.ExchangeRate;
            }

            invoice.Subtotal = invoice.Lines.Sum(l => l.Subtotal);
            invoice.TotalTax = invoice.Lines.Sum(l => l.TaxAmount);
            invoice.Total = invoice.Lines.Sum(l => l.Total);

            invoice.CurrencyId ??= invoice.Lines
                .Select(l => l.CurrencyId)
                .FirstOrDefault(id => id.HasValue);

            invoice.ExchangeRate ??= invoice.Lines
                .Select(l => l.ExchangeRate)
                .FirstOrDefault(rate => rate.HasValue && rate > 0m);
        }
    }
}
