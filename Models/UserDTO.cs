using System.Text.Json.Serialization;

namespace Models
{
    public class UserDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}