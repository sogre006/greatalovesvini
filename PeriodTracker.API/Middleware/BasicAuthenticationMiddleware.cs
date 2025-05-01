using System.Text;
using Microsoft.AspNetCore.Authorization;
using PeriodTracker.Model.Repositories;

namespace PeriodTracker.API.Middleware
{
    public class BasicAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UserRepository _userRepository;

        public BasicAuthenticationMiddleware(RequestDelegate next, UserRepository userRepository)
        {
            _next = next;
            _userRepository = userRepository;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Bypass authentication for [AllowAnonymous] endpoints
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            // Also bypass for login and register endpoints
            var path = context.Request.Path.ToString().ToLower();
            if (path.Contains("/login") || path.Contains("/register"))
            {
                await _next(context);
                return;
            }

            // 1. Try to retrieve the Authorization header
            string? authHeader = context.Request.Headers.Authorization;
            
            // 2. If not found, return 401 Unauthorized
            if (string.IsNullOrEmpty(authHeader))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authorization Header value not provided");
                Console.WriteLine("Auth failed: No Authorization header found");
                return;
            }

            try
            {
                // 3. Extract the encoded credentials from the value
                if (!authHeader.StartsWith("Basic "))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid Authorization header format");
                    Console.WriteLine("Auth failed: Header doesn't start with 'Basic '");
                    return;
                }

                var encodedCredentials = authHeader.Substring(6); // Skip "Basic "
                
                // 4. Decode from Base64
                var bytes = Convert.FromBase64String(encodedCredentials);
                var credentials = Encoding.UTF8.GetString(bytes);
                
                // 5. Extract username and password (separated by colon)
                var parts = credentials.Split(':');
                if (parts.Length != 2)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid credentials format");
                    return;
                }
                
                var username = parts[0];
                var password = parts[1];
                
                // 6. Check user credentials against database
                var validUser = false;
                
                // If it's our hardcoded test user
                if (username == "john.doe" && password == "VerySecret!")
                {
                    validUser = true;
                }
                else
                {
                    // Try to find user by email (username)
                    var user = _userRepository.GetUserByEmail(username);
                    if (user != null && user.pw == password) // In real app, use proper password verification
                    {
                        validUser = true;
                    }
                }
                
                if (validUser)
                {
                    // Authentication successful, continue to the next middleware
                    await _next(context);
                }
                else
                {
                    // Authentication failed
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid credentials");
                    return;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions during authentication
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Authentication error");
                Console.WriteLine($"Auth exception: {ex.Message}");
                return;
            }
        }
    }

    public static class BasicAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseBasicAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthenticationMiddleware>();
        }
    }
}