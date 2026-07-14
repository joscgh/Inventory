using Inventory.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Core.Classes
{
    public class ItemUniversal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; internal set; }
        public string SKU { get; set; } = string.Empty; // Código único del producto
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CurrencyId { get; set; }
        public Currency? Currency { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public double Stock { get; set; }
        public UnitType Unit { get; set; }

        // Aquí ocurre la magia: Cualquier dato extra va en esta lista
        public List<Attribute> Attributes { get; set; } = new List<Attribute>();

        public ItemUniversal() { }
        public ItemUniversal(string name, decimal price, double stock, UnitType unit, string sku, int currencyId, int categoryId)
        {
            Name = name;
            Price = price;
            Stock = stock;
            Unit = unit;
            SKU = sku;
            CurrencyId = currencyId;
            CategoryId = categoryId;
        }

        public void AddAttribute(string name, string value)
        {
            if (Attributes.All(a => !a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                Attributes.Add(new Attribute(name, value));
            }
        }

        public string GetAttribute(string name)
        {
            var attr = Attributes.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return attr?.Value ?? "N/A";
        }

        public decimal CalculateInventoryValue()
        {
            return Price * (decimal)Stock;
        }

        public void ShowTechnicalSheet()
        {
            Console.WriteLine($"=========================================");
            Console.WriteLine($"[{Category?.Name ?? "Uncategorized"}] {Name} (ID: {Id})");
            Console.WriteLine($"Price: {Currency?.Symbol ?? string.Empty}{Price} | Stock: {Stock} {Unit}");
            Console.WriteLine($"Specifications:");
            foreach (var attr in Attributes)
            {
                Console.WriteLine($"  - {attr.Name}: {attr.Value}");
            }
        }
    }
}