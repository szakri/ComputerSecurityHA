namespace Models
{
    public class CaffDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UploaderId { get; set; }
        public string UploaderUsername { get; set; }
        public new List<CommentDTO> Comments { get; set; }
    }
}