using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Models;

namespace TodoApp.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TodoItem>()
                .HasKey(t => t.Id);

            // Ensure Title is required (already done via attribute, but explicit config is good too)
            modelBuilder.Entity<TodoItem>()
                .Property(t => t.Title)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<TodoItem>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId);
        }
    }
}
