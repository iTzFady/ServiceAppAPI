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
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceRequest>()
                .Property(e => e.ImageUrls)
                .HasConversion(
                    v => string.Join(';', v),
                    v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            modelBuilder.Entity<User>()
                .Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            modelBuilder.Entity<User>()
                .Property(x => x.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(y => y.WorkerSpecialty)
                .HasConversion<string>();

            modelBuilder.Entity<Rating>()
                .HasOne(r => r.RatedBy)
                .WithMany(u => u.RatingsGiven)
                .HasForeignKey(r => r.RatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.RatedUser)
                .WithMany(u => u.RatingsReceived)
                .HasForeignKey(r => r.RatedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .Property(x => x.Id)
                .HasDefaultValueSql("gen_random_uuid()");
        }


    }
}
