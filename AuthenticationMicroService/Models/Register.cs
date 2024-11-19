namespace AuthenticationMicroService.Models
{
    public class Register
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; } // New property
    }
}
