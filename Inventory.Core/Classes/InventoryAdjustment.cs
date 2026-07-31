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

        /// <summary>Depósito o tienda donde ocurrió el movimiento. Null en los ajustes previos a esta función.</summary>
        public int? LocationId { get; set; }
        public AccountLocation? Location { get; set; }

        // Optional navigation
        public ItemUniversal? Item { get; set; }
    }
}
