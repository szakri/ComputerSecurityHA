using System.Text.Json.Serialization;

namespace Models
{
    public class CaffDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("uploaderId")]
        public string UploaderId { get; set; }
        [JsonPropertyName("uploaderUsername")]
        public string UploaderUsername { get; set; }
        [JsonPropertyName("comments")]
        public new List<CommentDTO> Comments { get; set; }
    }
}