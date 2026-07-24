using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface ITaxService
    {
        Task<IEnumerable<Tax>> ListTaxesAsync();
        Task<Tax?> FindByIdAsync(int id);
        Task<bool> RegisterTaxAsync(Tax tax);
        Task<bool> ModifyTaxAsync(Tax tax);
        Task<bool> RemoveTaxAsync(int id);
    }
}
