using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface IAccountLocationService
    {
        Task<IEnumerable<AccountLocation>> ListByAccountAsync(int accountId);
        Task<AccountLocation?> FindByIdAsync(int id);
        Task<(bool Success, string? Error)> RegisterLocationAsync(int accountId, AccountLocation location);
        Task<(bool Success, string? Error)> ModifyLocationAsync(int accountId, int locationId, AccountLocation location);
        Task<bool> RemoveLocationAsync(int accountId, int locationId);
    }
}
