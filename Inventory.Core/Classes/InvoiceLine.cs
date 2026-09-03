namespace Inventory.Core.Classes
{
    /// <summary>
    /// Línea de una factura. Calca a <see cref="NoteLine"/> en el cálculo de importes,
    /// pero guarda Description, UnitPrice y TaxRate como copia: si el producto cambia
    /// de nombre, precio o impuesto, la factura ya emitida no se altera.
    /// </summary>
    public class InvoiceLine
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public int? ItemUniversalId { get; set; }
        public ItemUniversal? Item { get; set; }

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public int? CurrencyId { get; set; }
        public Currency? Currency { get; set; }

        /// <summary>Código con el que se vendió (SKU o código de barras), congelado.</summary>
        public string Code { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }

        /// <summary>Descuento aplicado a la línea, en importe (no porcentaje).</summary>
        public decimal Discount { get; set; }

        /// <summary>Tasa de cambio congelada de la línea. Cae a la de la factura si es nula.</summary>
        public decimal? ExchangeRate { get; set; }

        public decimal Subtotal => (UnitPrice * Quantity) - Discount;
        public decimal TaxAmount => Subtotal * TaxRate / 100;
        public decimal Total => Subtotal + TaxAmount;

        private decimal Rate => ExchangeRate ?? Currency?.ExchangeRate ?? 1m;

        public decimal SubtotalBs => Subtotal * Rate;
        public decimal TaxAmountBs => TaxAmount * Rate;
        public decimal TotalBs => Total * Rate;
    }
}
