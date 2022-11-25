using System.Text.Json.Serialization;

namespace Models
{
    public class NewCommentDTO
    {
        [JsonPropertyName("caffId")]
        public string CaffId { get; set; }
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("commentText")]
        public string CommentText { get; set; }
    }
}
