using Inventory.API.Repositories;
using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public class TaxService : ITaxService
    {
        private readonly ITaxRepository _repository;

        public TaxService(ITaxRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Tax>> ListTaxesAsync() =>
            await _repository.GetAllAsync();

        public async Task<Tax?> FindByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<bool> RegisterTaxAsync(Tax tax)
        {
            if (string.IsNullOrWhiteSpace(tax.Name) || tax.Rate < 0 || tax.Rate > 100)
            {
                return false;
            }

            var existing = await _repository.GetByNameAsync(tax.Name.Trim());
            if (existing != null) return false;

            tax.Name = tax.Name.Trim();
            await _repository.AddAsync(tax);
            return true;
        }

        public async Task<bool> ModifyTaxAsync(Tax tax)
        {
            var existing = await _repository.GetByIdAsync(tax.Id);
            if (existing == null) return false;

            existing.Name = tax.Name.Trim();
            existing.Rate = tax.Rate;
            await _repository.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> RemoveTaxAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
