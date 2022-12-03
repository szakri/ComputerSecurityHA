using Microsoft.EntityFrameworkCore;
using Models;

namespace DAL
{
    public class CaffContext : DbContext
    {
        public CaffContext(DbContextOptions<CaffContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = default!;
		public DbSet<Caff> Caffs { get; set; } = default!;
		public DbSet<Comment> Comments { get; set; } = default!;
	}
}