using Microsoft.AspNetCore.Mvc;
using PeriodTracker.Model.Entities;
using PeriodTracker.Model.Repositories;

namespace PeriodTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Calendar : ControllerBase
    {
        private readonly CalendarRepository _calendarRepository;
        private readonly UserRepository _userRepository;

        public Calendar(CalendarRepository calendarRepository, UserRepository userRepository)
        {
            _calendarRepository = calendarRepository;
            _userRepository = userRepository;
        }

        // GET: api/calendar/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Model.Entities.Calendar> GetCalendarById(int id)
        {
            var calendar = _calendarRepository.GetById(id);
            if (calendar == null)
            {
                return NotFound($"Calendar with ID {id} not found");
            }
            
            return Ok(calendar);
        }

        // GET: api/calendar/user/{userId}
        [HttpGet("user/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<Model.Entities.Calendar>> GetCalendarsByUserId(int userId)
        {
            // Check if user exists
            var user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found");
            }
            
            var calendars = _calendarRepository.GetCalendarsByUserId(userId);
            return Ok(calendars);
        }

        // GET: api/calendar/user/{userId}/month/{month}/year/{year}
        [HttpGet("user/{userId}/month/{month}/year/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Model.Entities.Calendar> GetCalendarByMonthYear(int userId, short month, short year)
        {
            // Check if user exists
            var user = _userRepository.GetUserById(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found");
            }
            
            var calendar = _calendarRepository.GetByUserAndMonthYear(userId, month, year);
            if (calendar == null)
            {
                return NotFound($"Calendar for user ID {userId}, month {month}, year {year} not found");
            }
            
            return Ok(calendar);
        }

        // POST: api/calendar
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public ActionResult<Model.Entities.Calendar> CreateCalendar(Model.Entities.Calendar calendar)
        {
            // Check if user exists
            var user = _userRepository.GetUserById(calendar.userId);
            if (user == null)
            {
                return NotFound($"User with ID {calendar.userId} not found");
            }

            // Check if calendar with same month/year already exists for user
            var existingCalendar = _calendarRepository.GetByUserAndMonthYear(calendar.userId, calendar.month, calendar.year);
            if (existingCalendar != null)
            {
                return Conflict($"Calendar for month {calendar.month}, year {calendar.year} already exists for user with ID {calendar.userId}");
            }

            // Validate month (1-12)
            if (calendar.month < 1 || calendar.month > 12)
            {
                return BadRequest("Month must be between 1 and 12");
            }

            bool success = _calendarRepository.InsertCalendar(calendar);
            if (!success)
            {
                return BadRequest("Failed to create calendar");
            }

            return CreatedAtAction(nameof(GetCalendarById), new { id = calendar.calendarId }, calendar);
        }

        // DELETE: api/calendar/{id}/user/{userId}
        [HttpDelete("{id}/user/{userId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult DeleteCalendar(int id, int userId)
        {
            // Check if calendar exists
            var existingCalendar = _calendarRepository.GetById(id);
            if (existingCalendar == null)
            {
                return NotFound($"Calendar with ID {id} not found");
            }
            
            // Ensure user owns the calendar
            if (existingCalendar.userId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You can only delete your own calendars");
            }

            bool success = _calendarRepository.DeleteCalendar(id, userId);
            if (!success)
            {
                return BadRequest("Failed to delete calendar");
            }

            return NoContent();
        }
    }
}