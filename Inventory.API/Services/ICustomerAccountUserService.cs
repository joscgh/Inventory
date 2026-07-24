using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface ICustomerAccountUserService
    {
        Task<CustomerAccountUser?> FindByEmailAsync(string email);
        Task<CustomerAccountUser?> FindByIdAsync(int id);
        Task<bool> RegisterUserAsync(CustomerAccountUser user, string password);
        Task<bool> AssignUserToAccountAsync(int accountId, CustomerAccountUser user, string password);
        Task<bool> RemoveUserAsync(int id);
    }
}
