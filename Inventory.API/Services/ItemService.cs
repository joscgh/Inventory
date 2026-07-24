using Inventory.Core.Classes;
using Inventory.API.Repositories;

namespace Inventory.API.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _repository;
        private readonly Repositories.IAdjustmentRepository _adjustmentRepository;

        public ItemService(IItemRepository repository, Repositories.IAdjustmentRepository adjustmentRepository)
        {
            _repository = repository;
            _adjustmentRepository = adjustmentRepository;
        }

        public async Task<IEnumerable<ItemUniversal>> ListItemAsync() =>
            await _repository.GetAllAsync();

        public async Task<ItemUniversal?> FindByIdAsync(string id) =>
            await _repository.GetByIdAsync(id);

        public async Task<bool> RegisterItemAsync(ItemUniversal item)
        {
            if (item.Price < 0 || item.Stock < 0 || item.TaxRate < 0 || item.TaxRate > 100) return false;

            var existingItem = await _repository.GetByIdAsync(item.SKU);
            if (existingItem != null) return false;

            await _repository.AddAsync(item);
            return true;
        }

        public async Task<bool> ModifyItemAsync(ItemUniversal item)
        {
            if (item.Price < 0 || item.Stock < 0 || item.TaxRate < 0 || item.TaxRate > 100) return false;

            var existingItem = await _repository.GetByIdAsync(item.SKU);
            if (existingItem == null) return false;

            await _repository.UpdateAsync(item);
            return true;
        }

        public async Task<bool> AdjustStockAsync(string sku, double delta, string reason, string referenceType)
        {
            var existingItem = await _repository.GetByIdAsync(sku);
            if (existingItem == null) return false;

            var previousStock = existingItem.Stock;
            existingItem.Stock += delta;
            if (existingItem.Stock < 0) existingItem.Stock = 0;
            existingItem.Stock = Math.Round(existingItem.Stock, 2);

            // Usamos UpdateStockAsync (no UpdateAsync) para no reconstruir las
            // relaciones y evitar el borrado accidental de los PriceVariants.
            await _repository.UpdateStockAsync(existingItem);

            var adj = new Inventory.Core.Classes.InventoryAdjustment
            {
                ItemId = existingItem.Id,
                SKU = existingItem.SKU,
                Change = existingItem.Stock - previousStock,
                PreviousStock = previousStock,
                NewStock = existingItem.Stock,
                Reason = string.IsNullOrWhiteSpace(reason) ? "Stock adjustment" : reason,
                ReferenceType = string.IsNullOrWhiteSpace(referenceType) ? "Manual" : referenceType,
                Timestamp = DateTime.UtcNow
            };

            await _adjustmentRepository.AddAsync(adj);
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
