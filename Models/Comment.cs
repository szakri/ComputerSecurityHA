namespace Models
{
    public class Comment : Archivable
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public User User { get; set; }
    }
}