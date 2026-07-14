using System.Text.Json.Serialization;

namespace Inventory.Core.Classes
{
    public class Currency
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public decimal? ExchangeRate { get; set; }
        public DateTime? LastUpdated { get; set; }

        [JsonIgnore]
        public List<ItemUniversal> Items { get; set; } = new();
    }
}
