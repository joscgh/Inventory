using System;

namespace Inventory.Core.Classes
{
    public class InventoryAdjustment
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public double Change { get; set; }
        public double PreviousStock { get; set; }
        public double NewStock { get; set; }
        public string? Reason { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceId { get; set; }
        public string? User { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Optional navigation
        public ItemUniversal? Item { get; set; }
    }
}
