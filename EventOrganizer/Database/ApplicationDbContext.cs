using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;
namespace EventOrganizer.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        { }

        public DbSet<EventCategories> EventCategories { get; set; }
        public DbSet<EventThemes> EventThemes { get; set; }

    }
}

