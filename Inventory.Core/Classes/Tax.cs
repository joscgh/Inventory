using System.Text.Json.Serialization;

namespace Inventory.Core.Classes
{
    public class Tax
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Rate { get; set; }

        [JsonIgnore]
        public List<ItemUniversal> Items { get; set; } = new();
    }
}
