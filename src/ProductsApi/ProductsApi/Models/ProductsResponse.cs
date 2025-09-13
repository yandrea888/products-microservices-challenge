namespace ProductsApi.Models
{
    public class ProductsResponse
    {
        public List<Product> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }
}