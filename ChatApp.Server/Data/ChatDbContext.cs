using Microsoft.EntityFrameworkCore;
using ChatApp.Server.Models;

namespace ChatApp.Server.Data
{
    // The ChatDbContext class represents the database context for the chat application.
    public class ChatDbContext : DbContext
    {
        public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(user =>
            {
                user.Property(u => u.Username).HasMaxLength(32).IsRequired();
                user.HasIndex(u => u.Username).IsUnique();
            });

            modelBuilder.Entity<Message>(message =>
            {
                message.Property(m => m.Content).HasMaxLength(2000).IsRequired();
                message.HasIndex(m => m.SentAt);
                message.HasOne(m => m.User)
                    .WithMany()
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
