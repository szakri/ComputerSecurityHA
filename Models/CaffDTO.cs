using System.Text.Json.Serialization;

namespace Models
{
    public class CaffDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;
		[JsonPropertyName("name")]
        public string Name { get; set; } = default!;
		[JsonPropertyName("uploaderId")]
        public string UploaderId { get; set; } = default!;
		[JsonPropertyName("uploaderUsername")]
        public string UploaderUsername { get; set; } = default!;
		[JsonPropertyName("comments")]
        public List<CommentDTO> Comments { get; set; } = default!;
	}
}