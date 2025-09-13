namespace AuthServer.Models
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } = 3600;
    }
}