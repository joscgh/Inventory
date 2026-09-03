using Inventory.Core.Classes;
namespace Inventory.API.Repositories
{
    public interface IAdjustmentRepository
    {
        Task AddAsync(InventoryAdjustment adjustment);
        Task<List<InventoryAdjustment>> GetBySkuAsync(string sku);
        Task<List<InventoryAdjustment>> GetAllAsync();
    }
}
