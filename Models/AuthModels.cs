using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models
{
    // What the client sends to register
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";
    }

    // What the client sends to login
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }

    // What the API sends back after successful login or register
    public class AuthResponse
    {
        public string Token { get; set; } = "";   // JWT token — sent with every future request
        public string Email { get; set; } = "";   // logged in user's email
        public int UserId { get; set; }           // user's ID — React stores this to check post ownership
    }
}