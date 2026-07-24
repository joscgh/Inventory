using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface ICustomerAccountService
    {
        Task<IEnumerable<CustomerAccount>> ListAccountsAsync();
        Task<CustomerAccount?> FindByIdAsync(int id);
        Task<bool> RegisterAccountAsync(CustomerAccount account);
        Task<bool> ModifyAccountAsync(CustomerAccount account);
        Task<bool> RemoveAccountAsync(int id);
    }
}
