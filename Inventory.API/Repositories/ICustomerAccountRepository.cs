using Inventory.Core.Classes;

namespace Inventory.API.Repositories
{
    public interface ICustomerAccountRepository
    {
        Task<IEnumerable<CustomerAccount>> GetAllAsync();
        Task<CustomerAccount?> GetByIdAsync(int id);
        Task<CustomerAccount?> GetByEmailAsync(string email);
        Task AddAsync(CustomerAccount account);
        Task UpdateAsync(CustomerAccount account);
        Task DeleteAsync(int id);
    }
}
