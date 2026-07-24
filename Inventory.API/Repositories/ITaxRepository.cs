using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface ITaxRepository
    {
        Task<IEnumerable<Tax>> GetAllAsync();
        Task<Tax?> GetByIdAsync(int id);
        Task<Tax?> GetByNameAsync(string name);
        Task AddAsync(Tax tax);
        Task UpdateAsync(Tax tax);
        Task DeleteAsync(int id);
    }
}
