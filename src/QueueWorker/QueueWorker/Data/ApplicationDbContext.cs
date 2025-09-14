using Microsoft.EntityFrameworkCore;
using QueueWorker.Models;

namespace QueueWorker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<MessageQueue> MessageQueue { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ExternalId)
                .IsUnique(); 

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

              
            modelBuilder.Entity<MessageQueue>()
                .HasIndex(m => m.MessageId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}