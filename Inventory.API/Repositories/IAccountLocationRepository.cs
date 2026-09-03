using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface IAccountLocationRepository
    {
        Task<IEnumerable<AccountLocation>> GetByAccountAsync(int accountId);
        Task<AccountLocation?> GetByIdAsync(int id);
        Task<AccountLocation?> GetByNameAsync(int accountId, string name);
        Task AddAsync(AccountLocation location);
        Task UpdateAsync(AccountLocation location);
        Task DeleteAsync(int id);
    }
}
