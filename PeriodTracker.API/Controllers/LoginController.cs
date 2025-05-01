using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PeriodTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        // These credentials must match exactly what's in BasicAuthenticationMiddleware
        private const string USERNAME = "john.doe";
        private const string PASSWORD = "VerySecret!";
        
        [AllowAnonymous] // This is critical!
        [HttpPost]
        public ActionResult Login([FromBody] LoginRequest credentials)
        {
            // Check credentials
            if (credentials.Username == USERNAME && credentials.Password == PASSWORD)
            {
                // 1. Concatenate username and password with a colon
                var text = $"{credentials.Username}:{credentials.Password}";
                
                // 2. Base64encode the above using UTF8 encoding (important!)
                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                var encodedCredentials = Convert.ToBase64String(bytes);
                
                // 3. Prefix with "Basic " (note the space)
                var headerValue = $"Basic {encodedCredentials}";
                
                // Return the header value
                return Ok(new { headerValue = headerValue });
            }
            else
            {
                // Log the attempted values to help with debugging
                Console.WriteLine($"Login attempt failed. Received: {credentials.Username} / [password-hidden]");
                Console.WriteLine($"Expected: {USERNAME} / [password-hidden]");
                
                return Unauthorized();
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}