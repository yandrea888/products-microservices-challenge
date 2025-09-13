using System.ComponentModel.DataAnnotations;

namespace QueueWorker.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ExternalId { get; set; } = string.Empty;
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        
        public string Currency { get; set; } = "USD";
        
        public DateTime UpdatedAtUtc { get; set; }
        
        public DateTime ProcessedAtUtc { get; set; }
    }
}