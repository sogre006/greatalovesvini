// Change from LoginController.cs to AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeriodTracker.Model.Entities;
using PeriodTracker.Model.Repositories;

namespace PeriodTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        
        // These credentials must match exactly what's in BasicAuthenticationMiddleware
        private const string TEST_EMAIL = "john.doe";
        private const string TEST_PASSWORD = "VerySecret!";
        
        public AuthController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        
        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest credentials)
        {
            // Check credentials
            if (credentials.Email == TEST_EMAIL && credentials.Password == TEST_PASSWORD)
            {
                // 1. Concatenate email and password with a colon
                var text = $"{credentials.Email}:{credentials.Password}";
                
                // 2. Base64encode the above using UTF8 encoding
                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                var encodedCredentials = Convert.ToBase64String(bytes);
                
                // 3. Prefix with "Basic " (note the space)
                var headerValue = $"Basic {encodedCredentials}";
                
                // Return the header value
                return Ok(new { headerValue = headerValue });
            }
            else
            {
                // Check against database using email
                var user = _userRepository.GetUserByEmail(credentials.Email);
                
                if (user != null && user.pw == credentials.Password)
                {
                    // Create the auth header using email/password
                    var text = $"{credentials.Email}:{credentials.Password}";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                    var encodedCredentials = Convert.ToBase64String(bytes);
                    var headerValue = $"Basic {encodedCredentials}";
                    
                    return Ok(new { headerValue = headerValue });
                }
                
                // Log the attempted values to help with debugging
                Console.WriteLine($"Login attempt failed. Received: {credentials.Email} / [password-hidden]");
                
                return Unauthorized();
            }
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult Register([FromBody] RegisterRequest request)
        {
            // Check if email already exists
            if (_userRepository.EmailExists(request.Email))
            {
                return Conflict($"Email '{request.Email}' already exists");
            }

            // Create user object
            var user = new User
            {
                name = request.Name,
                email = request.Email,
                pw = request.Password,
                createdAt = DateTime.UtcNow
            };

            bool success = _userRepository.InsertUser(user);
            if (!success)
            {
                return BadRequest("Failed to create user");
            }

            return CreatedAtAction(nameof(Login), new { }, new { userId = user.userId, name = user.name, email = user.email });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}