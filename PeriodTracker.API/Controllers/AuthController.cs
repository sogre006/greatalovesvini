using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PeriodTracker.Model.Repositories;
using PeriodTracker.Model.Entities;
using System.Security.Cryptography;

namespace PeriodTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthController(UserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest request)
        {
            // Find user by username/email
            var user = _userRepository.GetUserByEmail(request.Username);

            // If user not found by email, try to find by name (username)
            if (user == null)
            {
                // For simplicity's sake, we're treating name as username
                var users = _userRepository.GetUsers();
                user = users.FirstOrDefault(u => u.name.Equals(request.Username));
            }

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            // Verify password (in a real app, would use hashing)
            if (!VerifyPassword(request.Password, user.pw))
            {
                return Unauthorized("Invalid username or password");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Return the token
            return Ok(new { token, user_id = user.userId });
        }

        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterRequest request)
        {
            // Check if email already exists
            if (_userRepository.EmailExists(request.Email))
            {
                return Conflict("Email already exists");
            }

            // Create new user
            var user = new User(0) // The repository will assign the actual ID
            {
                name = request.Name,
                email = request.Email,
                pw = HashPassword(request.Password), // In a real app, hash the password
                createdAt = DateTime.UtcNow
            };

            // Save user to database
            bool success = _userRepository.InsertUser(user);
            if (!success)
            {
                return BadRequest("Failed to create user");
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Return the token and user info
            return Ok(new { token, user_id = user.userId });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.userId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            // In a real application, use a proper password hashing library like BCrypt
            // This is a simple implementation for demonstration purposes
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string providedPassword, string storedPassword)
        {
            // In a real app, would use BCrypt or similar to verify
            // For now, just compare the passwords directly since we're not hashing in the sample
            return providedPassword == storedPassword;
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; } // Not used in this simplified implementation
        public string Password { get; set; }
    }
}