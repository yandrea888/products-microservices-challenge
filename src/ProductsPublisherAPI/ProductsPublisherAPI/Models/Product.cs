namespace ProductsPublisherAPI.Models
{
    public class Product
    {
        public string ExternalId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime UpdatedAtUtc { get; set; }
    }
}