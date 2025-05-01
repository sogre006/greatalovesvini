using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PeriodTracker.Model.Repositories;

namespace PeriodTracker.API.Middleware
{
    public class BasicAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public BasicAuthenticationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
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
            if (path.Contains("/login") || path.Contains("/register") || path.Contains("/user/exists"))
            {
                await _next(context);
                return;
            }

            // Get scoped UserRepository from the request services
            var userRepository = context.RequestServices.GetRequiredService<UserRepository>();

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
                
                // 5. Extract email and password (separated by colon)
                var parts = credentials.Split(':');
                if (parts.Length != 2)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid credentials format");
                    return;
                }
                
                var email = parts[0]; // This is now treated as the email
                var password = parts[1];
                
                Console.WriteLine($"Auth attempt with email: {email}");
                
                // 6. Check user credentials against database
                var validUser = false;
                
                // If it's our hardcoded test user
                if (email == "john.doe" && password == "VerySecret!")
                {
                    validUser = true;
                    Console.WriteLine("Test user authenticated successfully");
                }
                else
                {
                    // Try to find user by email
                    var user = userRepository.GetUserByEmail(email);
                    if (user != null)
                    {
                        Console.WriteLine($"Found user with email: {email}, ID: {user.userId}");
                        
                        if (user.pw == password) // In real app, use proper password verification
                        {
                            validUser = true;
                            Console.WriteLine("Password matches, user authenticated successfully");
                        }
                        else
                        {
                            Console.WriteLine("Password does not match");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No user found with email: {email}");
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
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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