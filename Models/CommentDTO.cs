namespace Models
{
    public class CommentDTO
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public UserDTO User { get; set; }
    }
}