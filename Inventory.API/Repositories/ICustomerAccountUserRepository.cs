using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface ICustomerAccountUserRepository
    {
        Task<CustomerAccountUser?> GetByEmailAsync(string email);
        Task<CustomerAccountUser?> GetByIdAsync(int id);
        Task AddAsync(CustomerAccountUser user);
        Task UpdateAsync(CustomerAccountUser user);
        Task DeleteAsync(int id);
    }
}
