using System.ComponentModel.DataAnnotations;

namespace QueueWorker.Models
{
    public class MessageQueue
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string MessageId { get; set; } = string.Empty;
        
        [Required]
        public string CorrelationId { get; set; } = string.Empty;
        
        [Required]
        public string MessageBody { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        
        public bool IsProcessed { get; set; } = false;
        
        public DateTime? ProcessedAt { get; set; }
    }
}