using Microsoft.Extensions.Configuration;
using PeriodTracker.Model.Entities;
using Npgsql;
using NpgsqlTypes;

namespace PeriodTracker.Model.Repositories
{
    public class CycleEntryRepository : BaseRepository
    {
        public CycleEntryRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public CycleEntry GetById(int id)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM CycleEntry WHERE entry_id = @id";
                cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
                
                var data = GetData(dbConn, cmd);
                if (data != null && data.Read())
                {
                    return new CycleEntry(Convert.ToInt32(data["entry_id"]))
                    {
                        CycleId = Convert.ToInt32(data["cycle_id"]),
                        CalendarId = Convert.ToInt32(data["calendar_id"]),
                        Date = Convert.ToDateTime(data["date"]),
                        CreatedAt = Convert.ToDateTime(data["created_at"])
                    };
                }
                return null;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public List<CycleEntry> GetEntriesByCycleId(int CycleId)
        {
            NpgsqlConnection dbConn = null;
            var entries = new List<CycleEntry>();
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM CycleEntry WHERE cycle_id = @CycleId ORDER BY Date";
                cmd.Parameters.Add("@CycleId", NpgsqlDbType.Integer).Value = CycleId;
                
                var data = GetData(dbConn, cmd);
                if (data != null)
                {
                    while (data.Read())
                    {
                        CycleEntry entry = new CycleEntry(Convert.ToInt32(data["entry_id"]))
                        {
                            CycleId = Convert.ToInt32(data["cycle_id"]),
                            CalendarId = Convert.ToInt32(data["calendar_id"]),
                            Date = Convert.ToDateTime(data["date"]),
                            CreatedAt = Convert.ToDateTime(data["created_at"])
                        };
                        entries.Add(entry);
                    }
                }
                return entries;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public List<CycleEntry> GetEntriesByCalendarId(int CalendarId)
        {
            NpgsqlConnection dbConn = null;
            var entries = new List<CycleEntry>();
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM CycleEntry WHERE calendar_id = @CalendarId ORDER BY Date";
                cmd.Parameters.Add("@CalendarId", NpgsqlDbType.Integer).Value = CalendarId;
                
                var data = GetData(dbConn, cmd);
                if (data != null)
                {
                    while (data.Read())
                    {
                        CycleEntry entry = new CycleEntry(Convert.ToInt32(data["entry_id"]))
                        {
                            CycleId = Convert.ToInt32(data["cycle_id"]),
                            CalendarId = Convert.ToInt32(data["calendar_id"]),
                            Date = Convert.ToDateTime(data["Date"]),
                            CreatedAt = Convert.ToDateTime(data["created_at"])
                        };
                        entries.Add(entry);
                    }
                }
                return entries;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool InsertEntry(CycleEntry entry)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO CycleEntry 
                    (cycle_id, calendar_id, date, created_at)
                    VALUES 
                    (@CycleId, @CalendarId, @Date, @CreatedAt)
                    RETURNING entry_id";
                
                cmd.Parameters.AddWithValue("@CycleId", NpgsqlDbType.Integer, entry.CycleId);
                cmd.Parameters.AddWithValue("@CalendarId", NpgsqlDbType.Integer, entry.CalendarId);
                cmd.Parameters.AddWithValue("@Date", NpgsqlDbType.Date, entry.Date);
                cmd.Parameters.AddWithValue("@CreatedAt", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
                
                dbConn.Open();
                // Get the newly created entry ID
                var entryId = Convert.ToInt32(cmd.ExecuteScalar());
                entry.EntryId = entryId;
                
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

        public bool DeleteEntry(int id)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "DELETE FROM CycleEntry WHERE entry_id = @id";
                cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
                
                bool result = DeleteData(dbConn, cmd);
                return result;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public List<CycleEntry> GetEntriesByDate(DateTime Date)
        {
            NpgsqlConnection dbConn = null;
            var entries = new List<CycleEntry>();
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM CycleEntry WHERE Date = @Date";
                cmd.Parameters.Add("@Date", NpgsqlDbType.Date).Value = Date;
                
                var data = GetData(dbConn, cmd);
                if (data != null)
                {
                    while (data.Read())
                    {
                        CycleEntry entry = new CycleEntry(Convert.ToInt32(data["entry_id"]))
                        {
                            CycleId = Convert.ToInt32(data["cycle_id"]),
                            CalendarId = Convert.ToInt32(data["calendar_id"]),
                            Date = Convert.ToDateTime(data["date"])
                        };
                        entries.Add(entry);
                    }
                }
                return entries;
            }
            finally
            {
                dbConn?.Close();
            }
        }
    }
}