using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface IItemService
    {
        Task<IEnumerable<ItemUniversal>> ListItemAsync();
        Task<ItemUniversal?> FindByIdAsync(string id);
        Task<bool> RegisterItemAsync(ItemUniversal item);
        Task<bool> ModifyItemAsync(ItemUniversal item);

        /// <summary>
        /// Ajusta las existencias en un depósito o tienda. Si no se indica ubicación
        /// y el artículo tiene existencias en una sola, se usa esa.
        /// </summary>
        Task<(bool Success, string? Error)> AdjustStockAsync(
            string sku, double delta, string reason, string referenceType, int? locationId);

        /// <summary>Fija la cantidad exacta de un artículo en un depósito o tienda.</summary>
        Task<(bool Success, string? Error)> SetStockAtLocationAsync(
            string sku, int locationId, double quantity, string reason);

        Task<IEnumerable<ItemStock>> ListStockByLocationAsync(string sku);
        Task<bool> RemoveItemAsync(string id);
        Task<decimal> GetTotalInventoryValueAsync();
    }
}
