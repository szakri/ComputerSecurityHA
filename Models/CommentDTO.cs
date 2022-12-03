using System.Text.Json.Serialization;

namespace Models
{
    public class CommentDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
		[JsonPropertyName("text")]
        public string Text { get; set; } = default!;
		[JsonPropertyName("user")]
        public UserDTO User { get; set; } = default!;
	}
}