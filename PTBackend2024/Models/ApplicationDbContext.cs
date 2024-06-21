using Microsoft.EntityFrameworkCore;

namespace PTBackend2024.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Photos)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); //Delete all user's photos with user

            modelBuilder.Entity<User>()
                .HasMany(u => u.Likes)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction); //No action, this is solved in userController

            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction); //No action, this is solved in userController

            modelBuilder.Entity<Photo>()
                .HasMany(p => p.Likes)
                .WithOne(l => l.Photo)
                .HasForeignKey(l => l.PhotoId)
                .OnDelete(DeleteBehavior.Cascade); // Delete all photo's likes with photo

            modelBuilder.Entity<Photo>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Photo)
                .HasForeignKey(c => c.PhotoId)
                .OnDelete(DeleteBehavior.Cascade); //Delete all photo's comments with photo
        }
    }
}
