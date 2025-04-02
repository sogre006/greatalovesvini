using PeriodTracker.Model.Entities;
using PeriodTracker.Model.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PeriodTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        protected UserRepository Repository { get; }

        public UserController(UserRepository repository)
        {
            Repository = repository;
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUser([FromRoute] int id)
        {
            User user = Repository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet] public ActionResult<IEnumerable<User>> GetUsers() 
        {
            return Ok(Repository.GetUsers());
        }

        [HttpPost]
        public ActionResult Post([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User info not correct");
            }

            bool status = Repository.InsertUser(user);
            if (status)
            {
                return Ok();
            }
            return BadRequest();
        }

        [HttpPut]
        public ActionResult UpdateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User info not correct");
            }

            User existingUser = Repository.GetUserById(user.Id);
            if (existingUser == null)
            {
                return NotFound($"User with id {user.Id} not found");
            }

            bool status = Repository.UpdateUser(user);
            if (status)
            {
                return Ok();
            }

            return BadRequest("Something went wrong");
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteUser([FromRoute] int id)
        {
            User existingUser = Repository.GetUserById(id);
            if (existingUser == null)
            {
                return NotFound($"User with id {id} not found");
            }

            bool status = Repository.DeleteUser(id);
            if (status)
            {
                return NoContent();
            }

            return BadRequest($"Unable to delete user with id {id}");
        }
    }
}

---

using Microsoft.AspNetCore.Mvc;
using PeriodTracker.Model.Entities;
using PeriodTracker.Model.Repositories;

namespace PeriodTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class User : ControllerBase
{
    private readonly UserRepository _userRepository;

    public User(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // GET: api/user
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Model.Entities.User>> GetUsers()
    {
        var users = _userRepository.GetUsers();
        return Ok(users);
    }

    // GET: api/user/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Model.Entities.User> GetUserById(int id)
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Model.Entities.User> GetUserByEmail(string email)
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
    public ActionResult<Model.Entities.User> CreateUser(Model.Entities.User user)
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult UpdateUser(Model.Entities.User user)
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult UpdatePassword(int userId, string password)
    {
        // Check if user exists
        var existingUser = _userRepository.GetUserById(userId);
        if (existingUser == null)
        {
            return NotFound($"User with ID {userId} not found");
        }

        bool success = _userRepository.UpdateUserPassword(userId, password);
        if (!success)
        {
            return BadRequest("Failed to update password");
        }

        return Ok("Password updated successfully");
    }

    // DELETE: api/user/{id}
    [HttpDelete("{id}")]
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