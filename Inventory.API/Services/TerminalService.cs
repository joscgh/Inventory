using Inventory.API.Repositories;
using Inventory.Core.Classes;
using Inventory.Core.Services;

namespace Inventory.API.Services
{
    public class TerminalService : ITerminalService
    {
        private readonly ITerminalRepository _repository;

        public TerminalService(ITerminalRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Terminal>> ListAsync(int? customerAccountId = null)
            => _repository.GetAllAsync(customerAccountId);

        public Task<Terminal?> FindByIdAsync(int id) => _repository.GetByIdAsync(id);

        public async Task<(Terminal? Terminal, string? Error)> CreateAsync(Terminal terminal)
        {
            if (terminal.CustomerAccountId <= 0)
            {
                return (null, "La caja debe pertenecer a una cuenta.");
            }

            if (string.IsNullOrWhiteSpace(terminal.Code))
            {
                return (null, "La caja necesita un código (por ejemplo A o CAJA1).");
            }

            if (!terminal.StoreId.HasValue)
            {
                return (null, "La caja debe tener una tienda emisora.");
            }

            var account = await _repository.GetAccountAsync(terminal.CustomerAccountId);
            if (account == null)
            {
                return (null, "La cuenta indicada no existe.");
            }

            if (terminal.StoreId is int storeId)
            {
                var store = await _repository.GetStoreAsync(storeId);
                if (store == null || store.Type != Inventory.Core.Enums.LocationType.Store)
                {
                    return (null, "La tienda emisora no existe o no es una tienda.");
                }

                if (store.CustomerAccountId != account.Id
                    || !string.Equals(
                        FiscalIdentifierValidator.NormalizeRif(store.Rif),
                        FiscalIdentifierValidator.NormalizeRif(account.Document),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return (null, "La tienda emisora debe pertenecer al mismo contribuyente que la cuenta de la caja.");
                }
            }

            if (string.IsNullOrWhiteSpace(terminal.Serie))
            {
                // Sin serie propia dos cajas compartirían numeración, que es justo lo
                // que este modelo existe para evitar.
                terminal.Serie = terminal.Code.Trim().ToUpperInvariant();
            }

            await _repository.AddAsync(terminal);
            return (terminal, null);
        }

        public Task<IEnumerable<TerminalPaymentMethod>> ListPaymentMethodsAsync(int terminalId) =>
            _repository.GetPaymentMethodsAsync(terminalId);

        public async Task<(bool Success, string? Error)> SavePaymentMethodsAsync(int terminalId, IEnumerable<TerminalPaymentMethod> methods)
        {
            if (await _repository.GetByIdAsync(terminalId) == null)
            {
                return (false, "La caja indicada no existe.");
            }

            var allowed = new[] { "cash", "card", "transfer", "mobile", "credit" };
            var normalized = methods
                .Where(method => allowed.Contains(method.Code, StringComparer.OrdinalIgnoreCase))
                .GroupBy(method => method.Code, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
            await _repository.SavePaymentMethodsAsync(terminalId, normalized);
            return (true, null);
        }

        public Task<IEnumerable<InvoiceNumberRange>> ListRangesAsync(int terminalId, InvoiceDocumentType? documentType = null)
            => _repository.GetRangesAsync(terminalId, documentType);

        public async Task<(InvoiceNumberRange? Range, string? Error)> AssignRangeAsync(int terminalId, RangeAssignmentRequest request)
        {
            if (request.Size <= 0)
            {
                return (null, "El tamaño del bloque debe ser mayor que cero.");
            }

            var terminal = await _repository.GetByIdAsync(terminalId);
            if (terminal == null)
            {
                return (null, "La caja indicada no existe.");
            }

            var last = await _repository.GetLastRangeAsync(terminalId, request.DocumentType);

            // El bloque nuevo arranca donde terminó el anterior, así la serie de la
            // caja queda continua aunque los bloques se entreguen con meses de por medio.
            var from = last == null ? 1 : last.ToNumber + 1;
            var controlFrom = request.ControlFromNumber
                ?? (last == null ? 1 : last.ControlFromNumber + (last.ToNumber - last.FromNumber + 1));

            var range = new InvoiceNumberRange
            {
                TerminalId = terminalId,
                DocumentType = request.DocumentType,
                Serie = terminal.Serie,
                FromNumber = from,
                ToNumber = from + request.Size - 1,
                NextNumber = from,
                ControlPrefix = request.ControlPrefix ?? string.Empty,
                ControlFromNumber = controlFrom,
                Authorization = request.Authorization,
                ExpiresAt = request.ExpiresAt,
                Status = InvoiceRangeStatus.Active,
                AssignedAtUtc = DateTime.UtcNow
            };

            await _repository.AddRangeAsync(range);
            return (range, null);
        }
    }
}
