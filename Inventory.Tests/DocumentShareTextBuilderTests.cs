using Inventory.Core.Services;

namespace Inventory.Tests;

public class DocumentShareTextBuilderTests
{
    [Fact]
    public void BuildNoteShareText_IncludesDocumentDetailsAndProducts()
    {
        var lines = new[]
        {
            (Description: "Producto A", Quantity: 2m, UnitPrice: 10m, TaxRate: 16m, SubtotalBs: 20m, TotalBs: 23.2m),
            (Description: "Producto B", Quantity: 1m, UnitPrice: 5m, TaxRate: 0m, SubtotalBs: 5m, TotalBs: 5m)
        };

        var text = DocumentShareTextBuilder.BuildNoteShareText(
            title: "NOTA DE ENTREGA",
            number: "ENT-20260803",
            issueDate: new DateTime(2026, 8, 3),
            customerName: "Juan Pérez",
            customerDocument: "V-12345678",
            customerAddress: "Av. Principal",
            notes: "Entrega a domicilio",
            warehouseName: "Almacén Central",
            storeName: "Tienda 1",
            lines: lines,
            subtotal: 25m,
            totalTax: 3.2m,
            grandTotal: 28.2m,
            validityPeriod: null);

        Assert.Contains("NOTA DE ENTREGA", text);
        Assert.Contains("ENT-20260803", text);
        Assert.Contains("Juan Pérez", text);
        Assert.Contains("Producto A", text);
        Assert.Contains("Total (Bs): 28.20", text);
    }
}
