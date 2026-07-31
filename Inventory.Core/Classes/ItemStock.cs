namespace Inventory.Core.Classes
{
    /// <summary>
    /// Existencias de un artículo en un depósito o tienda concreto.
    /// La suma de todas las filas de un artículo es <see cref="ItemUniversal.Stock"/>.
    /// </summary>
    public class ItemStock
    {
        public int ItemId { get; set; }
        public int LocationId { get; set; }
        public double Quantity { get; set; }

        public ItemUniversal? Item { get; set; }
        public AccountLocation? Location { get; set; }
    }
}
