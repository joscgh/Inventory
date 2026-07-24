using Inventory.API.Repositories;
using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public class CustomerAccountUserService : ICustomerAccountUserService
    {
        private readonly ICustomerAccountUserRepository _repository;
        private readonly ICustomerAccountRepository _accountRepository;

        public CustomerAccountUserService(
            ICustomerAccountUserRepository repository,
            ICustomerAccountRepository accountRepository)
        {
            _repository = repository;
            _accountRepository = accountRepository;
        }

        public async Task<CustomerAccountUser?> FindByEmailAsync(string email) =>
            await _repository.GetByEmailAsync(email);

        public async Task<CustomerAccountUser?> FindByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<bool> RegisterUserAsync(CustomerAccountUser user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(password))
                return false;

            var existing = await _repository.GetByEmailAsync(user.Email);
            if (existing != null) return false;

            user.PasswordHash = PasswordHasher.Hash(password);
            await _repository.AddAsync(user);
            return true;
        }

        public async Task<bool> AssignUserToAccountAsync(int accountId, CustomerAccountUser user, string password)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) return false;

            if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(password))
                return false;

            var existing = await _repository.GetByEmailAsync(user.Email);
            if (existing != null) return false;

            user.CustomerAccountId = accountId;
            user.PasswordHash = PasswordHasher.Hash(password);
            await _repository.AddAsync(user);
            return true;
        }

        public async Task<bool> RemoveUserAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
