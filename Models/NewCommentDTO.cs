using System.Text.Json.Serialization;

namespace Models
{
    public class NewCommentDTO
    {
        [JsonPropertyName("caffId")]
        public string CaffId { get; set; } = default!;
		[JsonPropertyName("userId")]
        public string UserId { get; set; } = default!;
		[JsonPropertyName("commentText")]
        public string CommentText { get; set; } = default!;
	}
}
