using Microsoft.Extensions.Configuration;
using PeriodTracker.Model.Entities;
using Npgsql;
using NpgsqlTypes;

namespace PeriodTracker.Model.Repositories
{
    public class CalendarRepository : BaseRepository
    {
        public CalendarRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public Calendar GetById(int id)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Calendar WHERE calendar_id = @id";
                cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
                
                var data = GetData(dbConn, cmd);
                if (data != null && data.Read())
                {
                    return new Calendar(Convert.ToInt32(data["calendar_id"]))
                    {
                        userId = Convert.ToInt32(data["user_id"]),
                        month = Convert.ToInt16(data["month"]),
                        year = Convert.ToInt16(data["year"]),
                        createdAt = Convert.ToDateTime(data["created_at"])
                    };
                }
                return null;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public Calendar GetByUserAndMonthYear(int userId, short month, short year)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Calendar WHERE user_id = @userId AND month = @month AND year = @year";
                cmd.Parameters.Add("@userId", NpgsqlDbType.Integer).Value = userId;
                cmd.Parameters.Add("@month", NpgsqlDbType.Smallint).Value = month;
                cmd.Parameters.Add("@year", NpgsqlDbType.Smallint).Value = year;
                
                var data = GetData(dbConn, cmd);
                if (data != null && data.Read())
                {
                    return new Calendar(Convert.ToInt32(data["calendar_id"]))
                    {
                        userId = Convert.ToInt32(data["user_id"]),
                        month = Convert.ToInt16(data["month"]),
                        year = Convert.ToInt16(data["year"]),
                        createdAt = Convert.ToDateTime(data["created_at"])
                    };
                }
                return null;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public List<Calendar> GetCalendarsByUserId(int userId)
        {
            NpgsqlConnection dbConn = null;
            var calendars = new List<Calendar>();
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Calendar WHERE user_id = @userId ORDER BY year DESC, month DESC";
                cmd.Parameters.Add("@userId", NpgsqlDbType.Integer).Value = userId;
                
                var data = GetData(dbConn, cmd);
                if (data != null)
                {
                    while (data.Read())
                    {
                        Calendar calendar = new Calendar(Convert.ToInt32(data["calendar_id"]))
                        {
                            userId = Convert.ToInt32(data["user_id"]),
                            month = Convert.ToInt16(data["month"]),
                            year = Convert.ToInt16(data["year"]),
                            createdAt = Convert.ToDateTime(data["created_at"])
                        };
                        calendars.Add(calendar);
                    }
                }
                return calendars;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool InsertCalendar(Calendar calendar)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Calendar 
                    (user_id, month, year, created_at)
                    VALUES 
                    (@userId, @month, @year, @createdAt)
                    RETURNING calendar_id";
                
                cmd.Parameters.AddWithValue("@userId", NpgsqlDbType.Integer, calendar.userId);
                cmd.Parameters.AddWithValue("@month", NpgsqlDbType.Smallint, calendar.month);
                cmd.Parameters.AddWithValue("@year", NpgsqlDbType.Smallint, calendar.year);
                cmd.Parameters.AddWithValue("@createdAt", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
                
                dbConn.Open();
                // Get the newly created calendar ID
                var calendarId = Convert.ToInt32(cmd.ExecuteScalar());
                calendar.calendarId = calendarId;
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool DeleteCalendar(int id, int userId)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "DELETE FROM Calendar WHERE calendar_id = @id AND user_id = @userId";
                cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
                cmd.Parameters.AddWithValue("@userId", NpgsqlDbType.Integer, userId);
                
                bool result = DeleteData(dbConn, cmd);
                return result;
            }
            finally
            {
                dbConn?.Close();
            }
        }
    }
}