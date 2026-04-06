using BlogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // base route → /api/auth
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config; // reads values from appsettings.json

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // POST /api/auth/register
        // Creates a new user, hashes their password, returns a JWT token
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // 400 + validation errors

            // Check if email is already registered
            if (_db.Users.Any(u => u.Email == model.Email))
                return BadRequest(new { message = "Email already registered." });

            // Hash the password before storing — never store plain text
            // BCrypt generates a unique salt and hashes automatically
            var user = new User
            {
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(); // commit new user to DB

            // Generate token and return immediately — user is logged in right after register
            // Same as Django's login() call after a successful register
            var token = GenerateToken(user);
            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id // React stores this to check post ownership
            });
        }

        // POST /api/auth/login
        // Verifies credentials and returns a JWT token
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find the user by email
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);

            // Verify password against the stored hash
            // BCrypt.Verify hashes the input and compares — never compares plain text
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid email or password." }); // 401

            var token = GenerateToken(user);
            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                UserId = user.Id
            });
        }

        // Generates a signed JWT token containing the user's ID and email as claims
        // Claims are key-value pairs embedded in the token — no DB lookup needed to read them
        // Like Django's session data but stateless
        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                // NameIdentifier claim holds the user's ID
                // Read in controllers with: User.FindFirst(ClaimTypes.NameIdentifier)
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7), // token valid for 7 days
                signingCredentials: creds
            );

            // Serialize token to a string — this is what gets sent to the client
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}