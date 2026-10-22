using System.Text.Json.Serialization;

namespace FrozenWorld.Models
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }
}