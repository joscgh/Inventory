using Inventory.Core.Classes;
using Inventory.SharedUI.Services;
using System.Net;
using System.Net.Http.Json;

namespace Inventory.Tests;

public class InvoiceApiServiceTests
{
    [Fact]
    public void BuildDraftInvoice_InitializesClientGuidAndTotals()
    {
        var item = new ItemUniversal
        {
            Id = 42,
            Name = "Laptop",
            Price = 100m,
            TaxRate = 16m,
            CurrencyId = 1,
            CategoryId = 2,
            SKU = "SKU-001"
        };

        var invoice = InvoiceApiService.BuildDraftInvoice(
            5,
            8,
            10,
            "Juan Pérez",
            "V-12345678",
            new[]
            {
                new InvoiceApiService.InvoiceLineDraft(item, 2m, 100m, 16m)
            });

        Assert.Equal(5, invoice.TerminalId);
        Assert.Equal(8, invoice.CustomerAccountId);
        Assert.Equal(10, invoice.CreatedByUserId);
        Assert.NotEqual(Guid.Empty, invoice.ClientGuid);
        Assert.Single(invoice.Lines);
        Assert.Equal(200m, invoice.Lines[0].Subtotal);
        Assert.Equal(32m, invoice.Lines[0].TaxAmount);
        Assert.Equal(232m, invoice.Lines[0].Total);
    }

    [Fact]
    public async Task CreateInvoiceAsync_PostsToInvoiceEndpointAndReadsResponse()
    {
        var expected = new Invoice
        {
            Id = 7,
            Serie = "POS-1",
            Number = 12,
            ControlNumber = "CTRL-12",
            Total = 116m
        };
        var handler = new RecordingHandler(expected);
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") };
        var service = new InvoiceApiService(http);

        var result = await service.CreateInvoiceAsync(new Invoice { ClientGuid = Guid.NewGuid(), Total = 116m });

        Assert.True(result.Success);
        Assert.Equal(expected.Id, result.Invoice?.Id);
        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/api/invoices", handler.RequestUri?.AbsolutePath);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Invoice response;

        public RecordingHandler(Invoice response)
        {
            this.response = response;
        }

        public HttpMethod? Method { get; private set; }
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(response)
            });
        }
    }
}
