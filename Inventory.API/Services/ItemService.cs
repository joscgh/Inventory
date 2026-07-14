using Inventory.Core.Classes;
using Inventory.API.Repositories;

namespace Inventory.API.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _repository;

        public ItemService(IItemRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ItemUniversal>> ListItemAsync() =>
            await _repository.GetAllAsync();

        public async Task<ItemUniversal?> FindByIdAsync(string id) =>
            await _repository.GetByIdAsync(id);

        public async Task<bool> RegisterItemAsync(ItemUniversal item)
        {
            if (item.Price < 0 || item.Stock < 0) return false;

            var existingItem = await _repository.GetByIdAsync(item.SKU);
            if (existingItem != null) return false;

            await _repository.AddAsync(item);
            return true;
        }

        public async Task<bool> ModifyItemAsync(ItemUniversal item)
        {
            var existingItem = await _repository.GetByIdAsync(item.SKU);
            if (existingItem == null) return false;

            await _repository.UpdateAsync(item);
            return true;
        }

        public async Task<bool> RemoveItemAsync(string id)
        {
            var existingItem = await _repository.GetByIdAsync(id);
            if (existingItem == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Sum(i => i.CalculateInventoryValue());
        }
    }
}
