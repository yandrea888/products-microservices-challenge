using ProductsPublisherAPI.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProductsPublisherAPI.Services
{
    public class ProductsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ProductsService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<Product>> GetProductsAsync(string accessToken)
        {
            var productsUrl = _configuration["ProductsApi:BaseUrl"] ?? "http://localhost:8082";
            
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.GetAsync($"{productsUrl}/api/Products");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var productsResponse = JsonSerializer.Deserialize<ProductsResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return productsResponse?.Items ?? new List<Product>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting products: {ex.Message}");
            }

            return new List<Product>();
        }
    }
}