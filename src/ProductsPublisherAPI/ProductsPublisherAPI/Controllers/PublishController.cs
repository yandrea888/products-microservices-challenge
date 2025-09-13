using Microsoft.AspNetCore.Mvc;
using ProductsPublisherAPI.Services;
using ProductsPublisherAPI.Data;
using ProductsPublisherAPI.Models;
using System.Text.Json;

namespace ProductsPublisherAPI.Controllers
{
    [ApiController]
    [Route("api/publish")]
    public class PublishController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ProductsService _productsService;
        private readonly ApplicationDbContext _dbContext;

        public PublishController(AuthService authService, ProductsService productsService, ApplicationDbContext dbContext)
        {
            _authService = authService;
            _productsService = productsService;
            _dbContext = dbContext;
        }

        [HttpPost("products")]
        public async Task<IActionResult> PublishProducts()
        {
            try
            {
                // 1. Obtener token del AuthServer
                Console.WriteLine("Getting access token from AuthServer...");
                var accessToken = await _authService.GetAccessTokenAsync();
                
                if (string.IsNullOrEmpty(accessToken))
                {
                    return BadRequest("Failed to get access token");
                }

                // 2. Obtener productos del ProductsApi
                Console.WriteLine("Getting products from ProductsApi...");
                var products = await _productsService.GetProductsAsync(accessToken);

                if (!products.Any())
                {
                    return BadRequest("No products found");
                }

                // 3. Escribir cada producto a la cola (base de datos)
                Console.WriteLine($"Writing {products.Count} products to message queue...");
                
                foreach (var product in products)
                {
                    var correlationId = Guid.NewGuid().ToString();
                    var messageId = $"msg-{product.ExternalId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                    
                    var message = new
                    {
                        ExternalId = product.ExternalId,
                        Name = product.Name,
                        Price = product.Price,
                        Currency = product.Currency,
                        FetchedAtUtc = DateTime.UtcNow,
                        CorrelationId = correlationId,
                        Source = "ProductsApi:/api/products"
                    };

                    var queueMessage = new MessageQueue
                    {
                        MessageId = messageId,
                        CorrelationId = correlationId,
                        MessageBody = JsonSerializer.Serialize(message),
                        CreatedAt = DateTime.UtcNow,
                        IsProcessed = false
                    };

                    _dbContext.MessageQueue.Add(queueMessage);
                    
                    Console.WriteLine($"Queued message: {messageId} - CorrelationId: {correlationId}");
                }

                await _dbContext.SaveChangesAsync();

                return Ok(new { 
                    Message = $"Successfully queued {products.Count} products",
                    ProductCount = products.Count 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}