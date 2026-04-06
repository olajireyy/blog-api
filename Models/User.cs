namespace BlogApi.Models
{
    // Simple user table — stores email and hashed password
    // For a full production app use ASP.NET Identity like in Project 2
    // Here we keep it simple to focus on JWT concepts
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = ""; // never store plain text passwords
    }
}