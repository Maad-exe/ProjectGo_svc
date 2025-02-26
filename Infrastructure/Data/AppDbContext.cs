using backend.Core.Entities;
using backend.Core.Enums;
using backend.Infrastructure.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace backend.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Teacher>().ToTable("Teachers");
            modelBuilder.Entity<Student>().ToTable("Students");

            modelBuilder.Entity<Admin>().HasData(new Admin
            {
                Id = 1,
                CreatedAt = new DateTime(2025, 2, 18, 12, 0, 0, DateTimeKind.Utc),
                Email = "admin@projectgo.com",
                FullName = "System Admin",
                PasswordHash = "$2a$11$dGbHOWMrjr/9KPl9LxongumrriovDITJb6H42vb3s4RpHAYURKE4C", // Static hash for "adminpassword"
                Role = UserType.Admin
            });

            // Apply configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        
    }
    }
}
