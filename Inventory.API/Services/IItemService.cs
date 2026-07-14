using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface IItemService
    {
        Task<IEnumerable<ItemUniversal>> ListItemAsync();
        Task<ItemUniversal?> FindByIdAsync(string id);
        Task<bool> RegisterItemAsync(ItemUniversal item);
        Task<bool> ModifyItemAsync(ItemUniversal item);
        Task<bool> RemoveItemAsync(string id);
        Task<decimal> GetTotalInventoryValueAsync();
    }
}
