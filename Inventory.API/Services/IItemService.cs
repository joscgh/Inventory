using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface IItemService
    {
        Task<IEnumerable<ItemUniversal>> ListItemAsync();
        Task<ItemUniversal?> FindByIdAsync(string id);
        Task<bool> RegisterItemAsync(ItemUniversal item);
        Task<bool> ModifyItemAsync(ItemUniversal item);
        Task<bool> AdjustStockAsync(string sku, double delta, string reason, string referenceType);
        Task<bool> RemoveItemAsync(string id);
        Task<decimal> GetTotalInventoryValueAsync();
    }
}
