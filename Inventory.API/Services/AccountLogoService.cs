using Inventory.API.Repositories;
using Inventory.Core.Classes;

namespace Inventory.API.Services
{
    public class AccountLogoService : IAccountLogoService
    {
        private readonly IAccountLogoRepository _repository;
        private readonly ICustomerAccountRepository _accountRepository;

        public AccountLogoService(
            IAccountLogoRepository repository,
            ICustomerAccountRepository accountRepository)
        {
            _repository = repository;
            _accountRepository = accountRepository;
        }

        public async Task<AccountLogo?> FindByAccountAsync(int accountId) =>
            await _repository.GetByAccountAsync(accountId);

        public async Task<(bool Success, string? Error)> SaveLogoAsync(int accountId, byte[] data, string fileName)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            if (account == null) return (false, "No se encontró la cuenta indicada.");

            if (data.Length == 0)
                return (false, "El archivo está vacío.");

            if (data.Length > IAccountLogoService.MaxSizeBytes)
                return (false, "La imagen no puede superar los 2 MB.");

            // El tipo se deduce del contenido, no de lo que declare el cliente:
            // así no se puede guardar un archivo cuyo content-type miente.
            var contentType = DetectImageContentType(data);
            if (contentType == null)
                return (false, "Formato no soportado. Usa PNG, JPG, GIF o WEBP.");

            await _repository.SaveAsync(new AccountLogo
            {
                CustomerAccountId = accountId,
                ContentType = contentType,
                FileName = Path.GetFileName(fileName ?? string.Empty),
                Data = data
            });

            return (true, null);
        }

        public async Task<bool> RemoveLogoAsync(int accountId)
        {
            var existing = await _repository.GetByAccountAsync(accountId);
            if (existing == null) return false;

            await _repository.DeleteAsync(accountId);
            return true;
        }

        /// <summary>
        /// Identifica el formato por su firma binaria. Devuelve null si no es una imagen soportada.
        /// </summary>
        private static string? DetectImageContentType(byte[] data)
        {
            if (data.Length >= 8 &&
                data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
                data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
            {
                return "image/png";
            }

            if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            {
                return "image/jpeg";
            }

            if (data.Length >= 6 &&
                data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38 &&
                (data[4] == 0x37 || data[4] == 0x39) && data[5] == 0x61)
            {
                return "image/gif";
            }

            if (data.Length >= 12 &&
                data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
            {
                return "image/webp";
            }

            return null;
        }
    }
}
