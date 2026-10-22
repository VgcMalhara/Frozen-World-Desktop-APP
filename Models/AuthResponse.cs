using System.Text.Json.Serialization;

namespace FrozenWorld.Models
{
    public class AuthResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("user")]
        public User? User { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }
}