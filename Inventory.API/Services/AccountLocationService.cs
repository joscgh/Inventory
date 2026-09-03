using Inventory.API.Repositories;
using Inventory.Core.Classes;
using Inventory.Core.Services;

namespace Inventory.API.Services
{
    public class AccountLocationService : IAccountLocationService
    {
        private readonly IAccountLocationRepository _repository;
        private readonly ICustomerAccountRepository _accountRepository;

        public AccountLocationService(
            IAccountLocationRepository repository,
            ICustomerAccountRepository accountRepository)
        {
            _repository = repository;
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<AccountLocation>> ListByAccountAsync(int accountId) =>
            await _repository.GetByAccountAsync(accountId);

        public async Task<AccountLocation?> FindByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<(bool Success, string? Error)> RegisterLocationAsync(int accountId, AccountLocation location)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) return (false, "No se encontró la cuenta indicada.");

            if (string.IsNullOrWhiteSpace(location.Name))
                return (false, "El nombre del depósito o tienda es obligatorio.");

            if (string.IsNullOrWhiteSpace(location.Address))
                return (false, "La dirección fiscal de la ubicación es obligatoria.");

            if (!FiscalIdentifierValidator.IsValidRif(location.Rif))
                return (false, "La ubicación debe tener un RIF válido.");

            if (!string.Equals(
                FiscalIdentifierValidator.NormalizeRif(location.Rif),
                FiscalIdentifierValidator.NormalizeRif(account.Document),
                StringComparison.OrdinalIgnoreCase))
                return (false, "El RIF de la ubicación debe coincidir con el RIF de la cuenta.");

            var duplicate = await _repository.GetByNameAsync(accountId, location.Name.Trim());
            if (duplicate != null)
                return (false, "Ya existe un depósito o tienda con ese nombre en esta cuenta.");

            location.CustomerAccountId = accountId;
            location.Name = location.Name.Trim();
            location.Address = location.Address.Trim();
            location.Rif = FiscalIdentifierValidator.NormalizeRif(location.Rif);
            location.Account = null;

            await _repository.AddAsync(location);
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> ModifyLocationAsync(int accountId, int locationId, AccountLocation location)
        {
            var existing = await _repository.GetByIdAsync(locationId);
            if (existing == null || existing.CustomerAccountId != accountId)
                return (false, "No se encontró el depósito o tienda para actualizar.");

            if (string.IsNullOrWhiteSpace(location.Name))
                return (false, "El nombre del depósito o tienda es obligatorio.");

            if (string.IsNullOrWhiteSpace(location.Address))
                return (false, "La dirección fiscal de la ubicación es obligatoria.");

            if (!FiscalIdentifierValidator.IsValidRif(location.Rif))
                return (false, "La ubicación debe tener un RIF válido.");

            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) return (false, "No se encontró la cuenta indicada.");

            if (!string.Equals(
                FiscalIdentifierValidator.NormalizeRif(location.Rif),
                FiscalIdentifierValidator.NormalizeRif(account.Document),
                StringComparison.OrdinalIgnoreCase))
                return (false, "El RIF de la ubicación debe coincidir con el RIF de la cuenta.");

            var duplicate = await _repository.GetByNameAsync(accountId, location.Name.Trim());
            if (duplicate != null && duplicate.Id != locationId)
                return (false, "Ya existe un depósito o tienda con ese nombre en esta cuenta.");

            location.Id = locationId;
            location.CustomerAccountId = accountId;
            location.Name = location.Name.Trim();
            location.Address = location.Address.Trim();
            location.Rif = FiscalIdentifierValidator.NormalizeRif(location.Rif);

            await _repository.UpdateAsync(location);
            return (true, null);
        }

        public async Task<bool> RemoveLocationAsync(int accountId, int locationId)
        {
            var existing = await _repository.GetByIdAsync(locationId);
            if (existing == null || existing.CustomerAccountId != accountId) return false;

            await _repository.DeleteAsync(locationId);
            return true;
        }
    }
}
