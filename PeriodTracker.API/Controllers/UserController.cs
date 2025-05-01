using Microsoft.AspNetCore.Mvc;
using PeriodTracker.Model.Entities;
using PeriodTracker.Model.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace PeriodTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/user
        [HttpGet]
        // Make this explicitly [Authorize] to ensure middleware is checking
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            Console.WriteLine("[UserController] GetUsers called");
            var users = _userRepository.GetUsers();
            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        // Make this explicitly [Authorize] to ensure middleware is checking
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<User> GetUserById(int id)
        {
            Console.WriteLine($"[UserController] GetUserById called with id: {id}");
            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                Console.WriteLine($"[UserController] User with ID {id} not found");
                return NotFound($"User with ID {id} not found");
            }
            return Ok(user);
        }

        // GET: api/user/byemail/{email}
        [HttpGet("byemail/{email}")]
        // Make this explicitly [AllowAnonymous] to bypass middleware entirely for testing
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<User> GetUserByEmail(string email)
        {
            Console.WriteLine($"[UserController] GetUserByEmail called with email: {email}");
            
            // Dump all request headers for debugging
            Console.WriteLine("[UserController] Request headers:");
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"  {header.Key}: {header.Value}");
            }
            
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                Console.WriteLine($"[UserController] User with email '{email}' not found");
                return NotFound($"User with email '{email}' not found");
            }
            
            // Mask password before returning
            Console.WriteLine($"[UserController] User found: {user.name}, ID: {user.userId}");
            return Ok(user);
        }

        // POST: api/user
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<User> CreateUser(User user)
        {
            Console.WriteLine($"[UserController] CreateUser called for email: {user.email}");
            
            // Check if email already exists
            if (_userRepository.EmailExists(user.email))
            {
                Console.WriteLine($"[UserController] Email '{user.email}' already exists");
                return Conflict($"Email '{user.email}' already exists");
            }

            bool success = _userRepository.InsertUser(user);
            if (!success)
            {
                Console.WriteLine($"[UserController] Failed to create user with email: {user.email}");
                return BadRequest("Failed to create user");
            }

            Console.WriteLine($"[UserController] User created successfully: {user.name}, ID: {user.userId}");
            return CreatedAtAction(nameof(GetUserById), new { id = user.userId }, user);
        }

        // PUT: api/user
        [HttpPut]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult UpdateUser(User user)
        {
            Console.WriteLine($"[UserController] UpdateUser called for ID: {user.userId}");
            
            // Check if user exists
            var existingUser = _userRepository.GetUserById(user.userId);
            if (existingUser == null)
            {
                Console.WriteLine($"[UserController] User with ID {user.userId} not found");
                return NotFound($"User with ID {user.userId} not found");
            }

            // Check if the new email is already taken by another user
            var userWithSameEmail = _userRepository.GetUserByEmail(user.email);
            if (userWithSameEmail != null && userWithSameEmail.userId != user.userId)
            {
                Console.WriteLine($"[UserController] Email '{user.email}' is already taken");
                return Conflict($"Email '{user.email}' is already taken");
            }

            bool success = _userRepository.UpdateUser(user);
            if (!success)
            {
                Console.WriteLine($"[UserController] Failed to update user with ID: {user.userId}");
                return BadRequest("Failed to update user");
            }

            Console.WriteLine($"[UserController] User updated successfully: {user.name}, ID: {user.userId}");
            return Ok(user);
        }

        // PUT: api/user/password
        [HttpPut("password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdatePassword([FromBody] PasswordUpdateRequest request)
        {
            Console.WriteLine($"[UserController] UpdatePassword called for ID: {request.UserId}");
            
            // Check if user exists
            var existingUser = _userRepository.GetUserById(request.UserId);
            if (existingUser == null)
            {
                Console.WriteLine($"[UserController] User with ID {request.UserId} not found");
                return NotFound($"User with ID {request.UserId} not found");
            }

            // In a real application, we would hash the password here
            bool success = _userRepository.UpdateUserPassword(request.UserId, request.Password);
            if (!success)
            {
                Console.WriteLine($"[UserController] Failed to update password for user ID: {request.UserId}");
                return BadRequest("Failed to update password");
            }

            Console.WriteLine($"[UserController] Password updated successfully for user ID: {request.UserId}");
            return Ok("Password updated successfully");
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteUser(int id)
        {
            Console.WriteLine($"[UserController] DeleteUser called for ID: {id}");
            
            // Check if user exists
            var existingUser = _userRepository.GetUserById(id);
            if (existingUser == null)
            {
                Console.WriteLine($"[UserController] User with ID {id} not found");
                return NotFound($"User with ID {id} not found");
            }

            bool success = _userRepository.DeleteUser(id);
            if (!success)
            {
                Console.WriteLine($"[UserController] Failed to delete user with ID: {id}");
                return BadRequest("Failed to delete user");
            }

            Console.WriteLine($"[UserController] User deleted successfully: ID: {id}");
            return NoContent();
        }

        // GET: api/user/exists/email/{email}
        [HttpGet("exists/email/{email}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<bool> EmailExists(string email)
        {
            Console.WriteLine($"[UserController] EmailExists called for email: {email}");
            var exists = _userRepository.EmailExists(email);
            Console.WriteLine($"[UserController] Email '{email}' exists: {exists}");
            return Ok(exists);
        }
    }

    public class PasswordUpdateRequest
    {
        public int UserId { get; set; }
        public string Password { get; set; }
    }
}