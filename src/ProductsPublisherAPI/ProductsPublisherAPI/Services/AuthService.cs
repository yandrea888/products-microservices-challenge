using ProductsPublisherAPI.Models;
using System.Text;
using System.Text.Json;

namespace ProductsPublisherAPI.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var authUrl = _configuration["AuthServer:BaseUrl"] ?? "http://localhost:8081";
            
            var request = new AuthTokenRequest
            {
                Username = "testuser",
                Password = "testpass"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{authUrl}/auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<AuthTokenResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return authResponse?.AccessToken;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token: {ex.Message}");
            }

            return null;
        }
    }
}