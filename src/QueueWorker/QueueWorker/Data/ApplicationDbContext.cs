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
            // Configurar Product
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ExternalId)
                .IsUnique(); // Para evitar duplicados

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Configurar MessageQueue  
            modelBuilder.Entity<MessageQueue>()
                .HasIndex(m => m.MessageId)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}