using Inventory.API.Repositories;
using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public class CustomerAccountService : ICustomerAccountService
    {
        private readonly ICustomerAccountRepository _repository;
        private readonly IAccountLogoRepository _logoRepository;

        public CustomerAccountService(
            ICustomerAccountRepository repository,
            IAccountLogoRepository logoRepository)
        {
            _repository = repository;
            _logoRepository = logoRepository;
        }

        public async Task<IEnumerable<CustomerAccount>> ListAccountsAsync()
        {
            var accounts = (await _repository.GetAllAsync()).ToList();

            // Una sola consulta de ids para marcar quién tiene logo, sin traer las imágenes.
            var withLogo = await _logoRepository.GetAccountIdsWithLogoAsync();
            foreach (var account in accounts)
            {
                account.HasLogo = withLogo.Contains(account.Id);
            }

            return accounts;
        }

        public async Task<CustomerAccount?> FindByIdAsync(int id)
        {
            var account = await _repository.GetByIdAsync(id);
            if (account == null) return null;

            account.HasLogo = await _logoRepository.GetByAccountAsync(id) != null;
            return account;
        }

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
