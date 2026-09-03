namespace Inventory.Core.Classes
{
    public class NoteLine
    {
        public int Id { get; set; }
        public int NoteId { get; set; }
        public int? ItemUniversalId { get; set; }
        public ItemUniversal? Item { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public int? CurrencyId { get; set; }
        public Currency? Currency { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal CommittedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public Note? Note { get; set; }

        public decimal Subtotal => UnitPrice * Quantity;
        public decimal SubtotalBs => Subtotal * (Currency?.ExchangeRate ?? Item?.Currency?.ExchangeRate ?? 1m);
        public decimal TaxAmount => Subtotal * TaxRate / 100;
        public decimal TaxAmountBs => TaxAmount * (Currency?.ExchangeRate ?? Item?.Currency?.ExchangeRate ?? 1m);
        public decimal Total => Subtotal + TaxAmount;
        public decimal TotalBs => Total * (Currency?.ExchangeRate ?? Item?.Currency?.ExchangeRate ?? 1m);
    }
}
