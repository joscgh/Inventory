using System;
using System.Collections.Generic;
using System.Linq;

namespace Inventory.Core.Services
{
    public static class DocumentShareTextBuilder
    {
        public static string BuildNoteShareText(
            string title,
            string number,
            DateTime issueDate,
            string customerName,
            string customerDocument,
            string customerAddress,
            string notes,
            string? warehouseName,
            string? storeName,
            IEnumerable<(string Description, decimal Quantity, decimal UnitPrice, decimal TaxRate, decimal SubtotalBs, decimal TotalBs)> lines,
            decimal subtotal,
            decimal totalTax,
            decimal grandTotal,
            string? validityPeriod)
        {
            var sections = new List<string>
            {
                title,
                $"Número: {number}",
                $"Fecha: {issueDate:dd/MM/yyyy}"
            };

            if (!string.IsNullOrWhiteSpace(customerName))
            {
                sections.Add($"Cliente: {customerName}");
            }

            if (!string.IsNullOrWhiteSpace(customerDocument))
            {
                sections.Add($"Documento: {customerDocument}");
            }

            if (!string.IsNullOrWhiteSpace(customerAddress))
            {
                sections.Add($"Dirección: {customerAddress}");
            }

            if (!string.IsNullOrWhiteSpace(warehouseName))
            {
                sections.Add($"Depósito: {warehouseName}");
            }

            if (!string.IsNullOrWhiteSpace(storeName))
            {
                sections.Add($"Tienda: {storeName}");
            }

            if (!string.IsNullOrWhiteSpace(validityPeriod))
            {
                sections.Add($"Vigencia: {validityPeriod}");
            }

            if (!string.IsNullOrWhiteSpace(notes))
            {
                sections.Add($"Observaciones: {notes}");
            }

            sections.Add("Productos:");
            foreach (var line in lines)
            {
                sections.Add($"- {line.Description}: Cant. {line.Quantity:N2} | P.U. {line.UnitPrice:N2} | Total Bs {line.TotalBs:N2}");
            }

            sections.Add($"Subtotal (Bs): {subtotal:N2}");
            sections.Add($"Total impuestos (Bs): {totalTax:N2}");
            sections.Add($"Total (Bs): {grandTotal:N2}");

            return string.Join(Environment.NewLine, sections);
        }
    }
}
