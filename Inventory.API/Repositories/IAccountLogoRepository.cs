using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface IAccountLogoRepository
    {
        Task<AccountLogo?> GetByAccountAsync(int accountId);
        Task<HashSet<int>> GetAccountIdsWithLogoAsync();
        Task SaveAsync(AccountLogo logo);
        Task DeleteAsync(int accountId);
    }
}
