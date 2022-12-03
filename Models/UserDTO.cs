using System.Text.Json.Serialization;

namespace Models
{
    public class UserDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
		[JsonPropertyName("username")]
        public string Username { get; set; } = default!;
	}
}