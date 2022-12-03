namespace Models
{
    public class User : Archivable
    {
        public int Id { get; set; } = default!;
		public string Username { get; set; } = default!;
		public string Password { get; set; } = default!;
	}
}