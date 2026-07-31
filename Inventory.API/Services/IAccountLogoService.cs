using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public interface IAccountLogoService
    {
        /// <summary>Tamaño máximo aceptado para un logo, en bytes.</summary>
        static int MaxSizeBytes => 2 * 1024 * 1024;

        Task<AccountLogo?> FindByAccountAsync(int accountId);
        Task<(bool Success, string? Error)> SaveLogoAsync(int accountId, byte[] data, string fileName);
        Task<bool> RemoveLogoAsync(int accountId);
    }
}
