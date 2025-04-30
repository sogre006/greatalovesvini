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
        [Authorize] // Require authentication
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            var users = _userRepository.GetUsers();
            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        [Authorize] // Require authentication
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<User> GetUserById(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }
            return Ok(user);
        }

        // GET: api/user/byemail/{email}
        [HttpGet("byemail/{email}")]
        [Authorize] // Require authentication
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<User> GetUserByEmail(string email)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound($"User with email '{email}' not found");
            }
            return Ok(user);
        }

        // POST: api/user
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<User> CreateUser(User user)
        {
            // Check if email already exists
            if (_userRepository.EmailExists(user.email))
            {
                return Conflict($"Email '{user.email}' already exists");
            }

            bool success = _userRepository.InsertUser(user);
            if (!success)
            {
                return BadRequest("Failed to create user");
            }

            return CreatedAtAction(nameof(GetUserById), new { id = user.userId }, user);
        }

        // PUT: api/user
        [HttpPut]
        [Authorize] // Require authentication
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult UpdateUser(User user)
        {
            // Check if user exists
            var existingUser = _userRepository.GetUserById(user.userId);
            if (existingUser == null)
            {
                return NotFound($"User with ID {user.userId} not found");
            }

            // Check if the new email is already taken by another user
            var userWithSameEmail = _userRepository.GetUserByEmail(user.email);
            if (userWithSameEmail != null && userWithSameEmail.userId != user.userId)
            {
                return Conflict($"Email '{user.email}' is already taken");
            }

            bool success = _userRepository.UpdateUser(user);
            if (!success)
            {
                return BadRequest("Failed to update user");
            }

            return Ok(user);
        }

        // PUT: api/user/password
        [HttpPut("password")]
        [Authorize] // Require authentication
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdatePassword([FromBody] PasswordUpdateRequest request)
        {
            // Check if user exists
            var existingUser = _userRepository.GetUserById(request.UserId);
            if (existingUser == null)
            {
                return NotFound($"User with ID {request.UserId} not found");
            }

            // In a real application, we would hash the password here
            bool success = _userRepository.UpdateUserPassword(request.UserId, request.Password);
            if (!success)
            {
                return BadRequest("Failed to update password");
            }

            return Ok("Password updated successfully");
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        [Authorize] // Require authentication
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteUser(int id)
        {
            // Check if user exists
            var existingUser = _userRepository.GetUserById(id);
            if (existingUser == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            bool success = _userRepository.DeleteUser(id);
            if (!success)
            {
                return BadRequest("Failed to delete user");
            }

            return NoContent();
        }

        // GET: api/user/exists/email/{email}
        [HttpGet("exists/email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<bool> EmailExists(string email)
        {
            return Ok(_userRepository.EmailExists(email));
        }
    }

    public class PasswordUpdateRequest
    {
        public int UserId { get; set; }
        public string Password { get; set; }
    }
}