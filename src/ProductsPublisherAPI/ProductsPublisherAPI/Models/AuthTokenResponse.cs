namespace ProductsPublisherAPI.Models
{
    public class AuthTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
    }
}