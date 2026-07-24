using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface IItemRepository
    {
        Task<IEnumerable<ItemUniversal>> GetAllAsync();
        Task<ItemUniversal?> GetByIdAsync(string SKU);
        Task AddAsync(ItemUniversal item);
        Task UpdateAsync(ItemUniversal item);
        Task UpdateStockAsync(ItemUniversal item);
        Task DeleteAsync(string SKU);
    }
}
