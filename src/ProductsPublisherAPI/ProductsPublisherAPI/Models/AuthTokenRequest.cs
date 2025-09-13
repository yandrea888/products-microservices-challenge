namespace ProductsPublisherAPI.Models
{
    public class AuthTokenRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}