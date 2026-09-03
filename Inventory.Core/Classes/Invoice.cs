using System;
using System.Collections.Generic;

namespace Inventory.Core.Classes
{
    public enum InvoiceDocumentType
    {
        Factura,
        NotaCredito,
        NotaDebito
    }

    public enum InvoiceStatus
    {
        Issued,
        Voided
    }

    public enum InvoiceEmissionMode
    {
        FormaLibre,
        Digital,
        MaquinaFiscal
    }

    /// <summary>
    /// Documento fiscal emitido. A diferencia de <see cref="Note"/>, una factura es
    /// inmutable: una vez emitida no se edita ni se reenumera. Para corregirla se
    /// emite una nota de crédito que la referencia.
    ///
    /// Los datos del cliente, la moneda y la tasa de cambio se copian aquí en vez de
    /// leerse por relación, porque la factura debe seguir mostrando lo que decía el
    /// día que se emitió aunque el cliente o la tasa cambien después.
    /// </summary>
    public class Invoice
    {
        public int Id { get; set; }

        /// <summary>
        /// Identificador que genera la caja al crear la factura. Es lo que hace
        /// idempotente la sincronización: si el POS reintenta subir la misma factura
        /// porque no llegó a ver la respuesta, el servidor la reconoce y no duplica.
        /// </summary>
        public Guid ClientGuid { get; set; }

        public InvoiceDocumentType DocumentType { get; set; } = InvoiceDocumentType.Factura;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;
        public InvoiceEmissionMode EmissionMode { get; set; } = InvoiceEmissionMode.FormaLibre;

        // --- Numeración fiscal ---
        public string Serie { get; set; } = string.Empty;
        public long Number { get; set; }
        public string ControlNumber { get; set; } = string.Empty;
        public string? FiscalDocumentId { get; set; }
        public string? FiscalDeviceSerial { get; set; }
        public string? FiscalAuthorizationNumber { get; set; }
        public string? FiscalDocumentHash { get; set; }

        public int TerminalId { get; set; }
        public Terminal? Terminal { get; set; }

        public int? InvoiceNumberRangeId { get; set; }
        public InvoiceNumberRange? NumberRange { get; set; }

        // --- Tiempos ---
        /// <summary>Momento en que la caja emitió la factura (su propio reloj).</summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>
        /// Momento en que el servidor la recibió. Con facturación offline puede ser
        /// mucho después de IssuedAt; la diferencia entre ambos es lo que permite
        /// auditar cuánto tiempo estuvo desconectada una caja.
        /// </summary>
        public DateTime ReceivedAtUtc { get; set; }

        // --- Emisor ---
        public int CustomerAccountId { get; set; }
        public CustomerAccount? CustomerAccount { get; set; }

        public int? StoreId { get; set; }
        public AccountLocation? Store { get; set; }

        /// <summary>Depósito del que sale la mercancía.</summary>
        public int? WarehouseId { get; set; }
        public AccountLocation? Warehouse { get; set; }

        public int CreatedByUserId { get; set; }
        public CustomerAccountUser? CreatedByUser { get; set; }

        // --- Receptor (congelado al emitir) ---
        public int? ConsumerCustomerId { get; set; }
        public ConsumerCustomer? ConsumerCustomer { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerDocument { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        // --- Importes ---
        public int? CurrencyId { get; set; }
        public Currency? Currency { get; set; }

        /// <summary>Tasa vigente al emitir. Se congela: la factura no se recalcula después.</summary>
        public decimal? ExchangeRate { get; set; }

        public decimal Subtotal { get; set; }
        public decimal TotalTax { get; set; }
        public decimal Total { get; set; }

        public string Notes { get; set; } = string.Empty;

        public List<InvoicePayment> Payments { get; set; } = new();

        // --- Anulación / referencia ---
        /// <summary>Factura que corrige este documento, cuando es nota de crédito o débito.</summary>
        public int? ReferenceInvoiceId { get; set; }
        public Invoice? ReferenceInvoice { get; set; }
        public List<Invoice> ReferencedByInvoices { get; set; } = new();

        public DateTime? VoidedAtUtc { get; set; }
        public string? VoidReason { get; set; }

        public List<InvoiceLine> Lines { get; set; } = new();
    }
}
