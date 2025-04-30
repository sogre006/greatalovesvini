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
        public ActionResult<Model.Entities.Calendar> GetCalendarByMonthYear(int userId, string month, short year)
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
            var user = _userRepository.GetUserById(calendar.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {calendar.UserId} not found");
            }

            // Check if calendar with same month/year already exists for user
            var existingCalendar = _calendarRepository.GetByUserAndMonthYear(calendar.UserId, calendar.Month.ToString(), calendar.Year);
            if (existingCalendar != null)
            {
                return Conflict($"Calendar for month {calendar.Month}, year {calendar.Year} already exists for user with ID {calendar.UserId}");
            }

            // Validate month (1-12)
            // Convert .Month to short before comparing
            if (Convert.ToInt16(calendar.Month) < 1 || Convert.ToInt16(calendar.Month) > 12) // <-- FIX
            {
                return BadRequest("Month must be between 1 and 12");
            }


            bool success = _calendarRepository.InsertCalendar(calendar);
            if (!success)
            {
                return BadRequest("Failed to create calendar");
            }

            return CreatedAtAction(nameof(GetCalendarById), new { id = calendar.CalendarId }, calendar);
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
            if (existingCalendar.UserId != userId)
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