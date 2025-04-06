using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AuthDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UsersRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(ur => ur.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(ur => ur.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Use static GUIDs for seed data
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "Admin"},
                new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "User" }
                );
        }
    }
}
