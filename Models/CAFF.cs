namespace Models
{
    public class Caff : Archivable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePathWithoutExtension { get; set; }
        public User Uploader { get; set; }
        public List<Comment> Comments { get; set; }
    }
}