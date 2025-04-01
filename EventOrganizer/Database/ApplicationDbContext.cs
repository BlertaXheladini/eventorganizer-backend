using Microsoft.EntityFrameworkCore;
namespace EventOrganizer.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        { }

        public DbSet<EventCategories> EventCategories { get; set; }
    }
}

