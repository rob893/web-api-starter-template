namespace ProjectTemplate.Models.DTOs
{
    public class LoginForReturnDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public UserForReturnDto User { get; set; }
    }
}