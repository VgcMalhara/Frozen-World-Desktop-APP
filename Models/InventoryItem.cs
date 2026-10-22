using System.Text.Json.Serialization;

namespace FrozenWorld.Models
{
    public class InventoryItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public string PriceString { get; set; } = string.Empty;

        [JsonIgnore]
        public decimal Price
        {
            get => decimal.TryParse(PriceString, out var p) ? p : 0;
            set => PriceString = value.ToString("0.00");
        }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }
}