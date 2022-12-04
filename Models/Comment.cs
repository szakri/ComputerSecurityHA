namespace Models
{
    public class Comment : Archivable
    {
        public int Id { get; set; } = default!;
        public string Text { get; set; } = default!;
		public User User { get; set; } = default!;
    }
}