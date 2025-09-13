using Microsoft.EntityFrameworkCore;
using ProductsPublisherAPI.Models;

namespace ProductsPublisherAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MessageQueue> MessageQueue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageQueue>()
                .HasIndex(m => m.MessageId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}