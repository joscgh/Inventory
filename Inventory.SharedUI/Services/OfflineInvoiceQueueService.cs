using Inventory.Core.Classes;

namespace Inventory.SharedUI.Services
{
    public sealed class OfflineInvoiceQueueService
    {
        private const string QueueKey = "inventory.pending-invoices";
        private readonly OfflineStorageService _storage;
        private readonly InvoiceApiService _invoiceApi;

        public OfflineInvoiceQueueService(OfflineStorageService storage, InvoiceApiService invoiceApi)
        {
            _storage = storage;
            _invoiceApi = invoiceApi;
        }

        public async Task QueueAsync(Invoice invoice)
        {
            var queue = await _storage.ReadAsync<List<Invoice>>(QueueKey) ?? new();
            queue.RemoveAll(item => item.ClientGuid == invoice.ClientGuid);
            queue.Add(invoice);
            await _storage.WriteAsync(QueueKey, queue);
        }

        public async Task<int> CountAsync()
            => (await _storage.ReadAsync<List<Invoice>>(QueueKey))?.Count ?? 0;

        public async Task<(int Synchronized, int Remaining)> SynchronizeAsync()
        {
            if (!await _storage.IsOnlineAsync())
            {
                return (0, await CountAsync());
            }

            var queue = await _storage.ReadAsync<List<Invoice>>(QueueKey) ?? new();
            var synchronized = 0;
            foreach (var invoice in queue.ToList())
            {
                var result = await _invoiceApi.CreateInvoiceAsync(invoice);
                if (result.Success)
                {
                    queue.Remove(invoice);
                    synchronized++;
                }
            }

            await _storage.WriteAsync(QueueKey, queue);
            return (synchronized, queue.Count);
        }
    }
}