namespace Models
{
    public class Caff : Archivable
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string FilePathWithoutExtension { get; set; } = default!;
		public User Uploader { get; set; } = default!;
		public List<Comment> Comments { get; set; } = default!;
	}
}