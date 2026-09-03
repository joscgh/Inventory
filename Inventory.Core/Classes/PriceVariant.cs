using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Core.Classes
{
    public class PriceVariant
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Label { get; set; } = string.Empty; // e.g., "Retail", "Wholesale"
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public Inventory.Core.Classes.ItemUniversal? Item { get; set; }
    }
}
