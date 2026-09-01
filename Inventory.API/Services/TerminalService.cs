using Inventory.API.Repositories;
using Inventory.Core.Classes;

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

            if (string.IsNullOrWhiteSpace(terminal.Serie))
            {
                // Sin serie propia dos cajas compartirían numeración, que es justo lo
                // que este modelo existe para evitar.
                terminal.Serie = terminal.Code.Trim().ToUpperInvariant();
            }

            await _repository.AddAsync(terminal);
            return (terminal, null);
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
