using Inventory.API.Repositories;
using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public class CustomerAccountService : ICustomerAccountService
    {
        private readonly ICustomerAccountRepository _repository;

        public CustomerAccountService(ICustomerAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CustomerAccount>> ListAccountsAsync() =>
            await _repository.GetAllAsync();

        public async Task<CustomerAccount?> FindByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<bool> RegisterAccountAsync(CustomerAccount account)
        {
            if (string.IsNullOrWhiteSpace(account.Name) || string.IsNullOrWhiteSpace(account.Email))
                return false;

            var existing = await _repository.GetByEmailAsync(account.Email);
            if (existing != null) return false;

            await _repository.AddAsync(account);
            return true;
        }

        public async Task<bool> ModifyAccountAsync(CustomerAccount account)
        {
            var existing = await _repository.GetByIdAsync(account.Id);
            if (existing == null) return false;

            await _repository.UpdateAsync(account);
            return true;
        }

        public async Task<bool> RemoveAccountAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
