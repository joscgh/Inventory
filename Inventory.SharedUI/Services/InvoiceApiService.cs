using System.Net;
using System.Net.Http.Json;
using Inventory.Core.Classes;

namespace Inventory.SharedUI.Services
{
    public class InvoiceApiService
    {
        private readonly HttpClient _http;

        public InvoiceApiService(HttpClient http)
        {
            _http = http;
        }

        public static Invoice BuildDraftInvoice(
            int terminalId,
            int customerAccountId,
            int createdByUserId,
            string customerName,
            string customerDocument,
            IEnumerable<InvoiceLineDraft> lineDrafts)
        {
            var invoice = new Invoice
            {
                ClientGuid = Guid.NewGuid(),
                TerminalId = terminalId,
                CustomerAccountId = customerAccountId,
                CreatedByUserId = createdByUserId,
                CustomerName = customerName,
                CustomerDocument = customerDocument,
                IssuedAt = DateTime.Now,
                Status = InvoiceStatus.Issued,
                DocumentType = InvoiceDocumentType.Factura
            };

            foreach (var draft in lineDrafts)
            {
                var item = draft.Item;
                var line = new InvoiceLine
                {
                    ItemUniversalId = item.Id,
                    Item = item,
                    Code = item.SKU,
                    Description = string.IsNullOrWhiteSpace(draft.Description) ? item.Name : draft.Description,
                    Quantity = draft.Quantity,
                    UnitPrice = draft.UnitPrice,
                    TaxRate = draft.TaxRate,
                    CurrencyId = item.CurrencyId,
                    ExchangeRate = 1m,
                    CategoryId = item.CategoryId,
                    Discount = draft.Discount
                };

                invoice.Lines.Add(line);
            }

            invoice.Subtotal = invoice.Lines.Sum(l => l.Subtotal);
            invoice.TotalTax = invoice.Lines.Sum(l => l.TaxAmount);
            invoice.Total = invoice.Lines.Sum(l => l.Total);
            invoice.CurrencyId ??= invoice.Lines
                .Select(l => l.CurrencyId)
                .FirstOrDefault(id => id.HasValue);
            invoice.ExchangeRate ??= invoice.Lines
                .Select(l => l.ExchangeRate)
                .FirstOrDefault(rate => rate.HasValue && rate > 0m);

            return invoice;
        }

        public async Task<List<Terminal>> GetTerminalsAsync(int? customerAccountId = null)
        {
            var url = customerAccountId.HasValue ? $"api/terminals?customerAccountId={customerAccountId.Value}" : "api/terminals";
            return await _http.GetFromJsonAsync<List<Terminal>>(url) ?? new();
        }

        public async Task<List<Invoice>> GetInvoicesAsync(
            InvoiceDocumentType? documentType = null,
            int? terminalId = null,
            int? customerAccountId = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            var query = new List<string>();

            if (documentType.HasValue) query.Add($"documentType={(int)documentType.Value}");
            if (terminalId.HasValue) query.Add($"terminalId={terminalId.Value}");
            if (customerAccountId.HasValue) query.Add($"customerAccountId={customerAccountId.Value}");
            if (from.HasValue) query.Add($"from={Uri.EscapeDataString(from.Value.ToString("O"))}");
            if (to.HasValue) query.Add($"to={Uri.EscapeDataString(to.Value.ToString("O"))}");

            var url = query.Count == 0 ? "api/invoices" : $"api/invoices?{string.Join("&", query)}";
            return await _http.GetFromJsonAsync<List<Invoice>>(url) ?? new();
        }

        public async Task<(bool Success, Invoice? Invoice, string? ErrorMessage)> CreateInvoiceAsync(Invoice invoice)
        {
            var response = await _http.PostAsJsonAsync("api/invoices", invoice);
            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<Invoice>();
                return (true, created, null);
            }

            var errorText = await response.Content.ReadAsStringAsync();
            var message = string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText;
            return (false, null, message);
        }

        public async Task<(bool Success, Invoice? Invoice, string? ErrorMessage)> VoidInvoiceAsync(int invoiceId, string reason)
        {
            var response = await _http.PostAsJsonAsync($"api/invoices/{invoiceId}/void", new { reason });
            if (response.IsSuccessStatusCode)
            {
                var invoice = await response.Content.ReadFromJsonAsync<Invoice>();
                return (true, invoice, null);
            }

            var errorText = await response.Content.ReadAsStringAsync();
            var message = string.IsNullOrWhiteSpace(errorText) ? response.ReasonPhrase : errorText;
            return (false, null, message);
        }

        public sealed record InvoiceLineDraft(
            ItemUniversal Item,
            decimal Quantity,
            decimal UnitPrice,
            decimal TaxRate,
            decimal Discount = 0m,
            string? Description = null);
    }
}
