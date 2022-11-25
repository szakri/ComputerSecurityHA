using System.Text.Json.Serialization;

namespace Models
{
    public class CommentDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("user")]
        public UserDTO User { get; set; }
    }
}