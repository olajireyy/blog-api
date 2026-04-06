using Microsoft.EntityFrameworkCore;

namespace BlogApi.Models
{
    // DbContext is the Django ORM equivalent — manages DB connection and queries
    public class AppDbContext : DbContext
    {
        // Constructor passes options up to the base DbContext
        // ASP.NET calls this automatically — you never instantiate it yourself
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet tells EF to create a Posts table mapped to the Post class
        public DbSet<Post> Posts { get; set; }

        // DbSet tells EF to create a Users table mapped to the User class
        public DbSet<User> Users { get; set; }
    }
}