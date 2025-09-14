using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsApi.Models;

namespace ProductsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetProducts(int page = 1, int pageSize = 50)
        {            
            var allProducts = GenerateSampleProducts();
            
            var totalCount = allProducts.Count;
            var pagedProducts = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new ProductsResponse
            {
                Items = pagedProducts,
                Page = page,
                PageSize = pageSize,
                Total = totalCount
            };

            return Ok(response);
        }

        private List<Product> GenerateSampleProducts()
        {
            return new List<Product>
            {
                new Product { ExternalId = "p-1001", Name = "Mouse", Price = 15.99m, Currency = "USD", UpdatedAtUtc = DateTime.UtcNow },
                new Product { ExternalId = "p-1002", Name = "Keyboard", Price = 45.50m, Currency = "USD", UpdatedAtUtc = DateTime.UtcNow },
                new Product { ExternalId = "p-1003", Name = "Monitor", Price = 199.99m, Currency = "USD", UpdatedAtUtc = DateTime.UtcNow },
                new Product { ExternalId = "p-1004", Name = "Headphones", Price = 89.99m, Currency = "USD", UpdatedAtUtc = DateTime.UtcNow },
                new Product { ExternalId = "p-1005", Name = "Webcam", Price = 75.00m, Currency = "USD", UpdatedAtUtc = DateTime.UtcNow }
            };
        }
    }
}