using Microsoft.EntityFrameworkCore;
using EventOrganizer.Models;

namespace EventOrganizer.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        { }

        public DbSet<Staff> Staff { get; set; }
        public DbSet<Events> Events { get; set; }
        public DbSet<EventCategories> EventCategories { get; set; }
        public DbSet<EventThemes> EventThemes { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Contact> Contact { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Reservations> Reservations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Events>()
                .HasOne(p => p.EventThemes)
                .WithMany()
                .HasForeignKey(p => p.ThemeId) // Foreign Key
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Events>()
                .HasOne(p => p.EventCategories)
                .WithMany()
                .HasForeignKey(p => p.CategoryId) // Foreign Key
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(p => p.Role)
                .WithMany()
                .HasForeignKey(p => p.RoleId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Feedback>()
                .HasOne(p => p.Events)
                .WithMany()
                .HasForeignKey(p => p.EventsId) //Foreign Key
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservations>()
           .HasOne(p => p.User)
           .WithMany()
           .HasForeignKey(p => p.UserID)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservations>()
           .HasOne(p => p.Event)
           .WithMany()
           .HasForeignKey(p => p.EventID)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservations>()
             .Property(r => r.ReservationDate)
             .HasConversion(
             v => v.ToDateTime(new TimeOnly()),  // Konverto DateOnly në DateTime
             v => DateOnly.FromDateTime(v)       // Konverto DateTime në DateOnly
             );
        }

        // Add the OnConfiguring method here to suppress warnings
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}