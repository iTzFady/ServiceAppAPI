using Microsoft.EntityFrameworkCore;
using ServiceApp.Models;

namespace ServiceApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Report> Reports { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<User>()
                .Property(x => x.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(y => y.WorkerSpecialty)
                .HasConversion<string>();
        }

    }
}
