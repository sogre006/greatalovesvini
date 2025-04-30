using Microsoft.Extensions.Configuration;
using PeriodTracker.Model.Entities;
using Npgsql;
using NpgsqlTypes;

namespace PeriodTracker.Model.Repositories
{
    public class PeriodCycleRepository : BaseRepository
    {
        public PeriodCycleRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public PeriodCycle GetById(int id)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM PeriodCycle WHERE cycle_id = @id";
                cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
                
                var data = GetData(dbConn, cmd);
                if (data != null && data.Read())
                {
                    return new PeriodCycle(Convert.ToInt32(data["cycle_id"]))
                    {
                        UserId = Convert.ToInt32(data["user_id"]),
                        StartDate = Convert.ToDateTime(data["start_date"]),
                        EndDate = Convert.ToDateTime(data["end_date"]),
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

        public List<PeriodCycle> GetCyclesByUserId(int UserId)
        {
            NpgsqlConnection dbConn = null;
            var cycles = new List<PeriodCycle>();
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM PeriodCycle WHERE user_id = @UserId ORDER BY start_date DESC";
                cmd.Parameters.Add("@UserId", NpgsqlDbType.Integer).Value = UserId;
                
                var data = GetData(dbConn, cmd);
                if (data != null)
                {
                    while (data.Read())
                    {
                        PeriodCycle cycle = new PeriodCycle(Convert.ToInt32(data["cycle_id"]))
                        {
                            UserId = Convert.ToInt32(data["user_id"]),
                            StartDate = Convert.ToDateTime(data["start_date"]),
                            EndDate = Convert.ToDateTime(data["end_date"]),
                            CreatedAt = Convert.ToDateTime(data["created_at"])
                        };
                        cycles.Add(cycle);
                    }
                }
                return cycles;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool InsertCycle(PeriodCycle cycle)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO PeriodCycle 
                    (user_id, start_date, end_date, created_at)
                    VALUES 
                    (@UserId, @StartDate, @EndDate, @CreatedAt)
                    RETURNING cycle_id";
                
                cmd.Parameters.AddWithValue("@UserId", NpgsqlDbType.Integer, cycle.UserId);
                cmd.Parameters.AddWithValue("@StartDate", NpgsqlDbType.Date, cycle.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", NpgsqlDbType.Date, cycle.EndDate);
                cmd.Parameters.AddWithValue("@CreatedAt", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
                
                dbConn.Open();
                // Get the newly created cycle ID
                var cycleId = Convert.ToInt32(cmd.ExecuteScalar());
                cycle.CycleId = cycleId;
                
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

        public bool UpdateCycle(PeriodCycle cycle)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE PeriodCycle SET
                    start_date = @StartDate,
                    end_date = @EndDate
                    WHERE cycle_id = @cycleId AND user_id = @UserId";
                
                cmd.Parameters.AddWithValue("@StartDate", NpgsqlDbType.Date, cycle.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", NpgsqlDbType.Date, cycle.EndDate);
                cmd.Parameters.AddWithValue("@cycleId", NpgsqlDbType.Integer, cycle.CycleId);
                cmd.Parameters.AddWithValue("@UserId", NpgsqlDbType.Integer, cycle.UserId);
                
                bool result = UpdateData(dbConn, cmd);
                return result;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool DeleteCycle(int id, int UserId)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "DELETE FROM PeriodCycle WHERE cycle_id = @id AND user_id = @UserId";
                cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
                cmd.Parameters.AddWithValue("@UserId", NpgsqlDbType.Integer, UserId);
                
                bool result = DeleteData(dbConn, cmd);
                return result;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public double GetAverageCycleDuration(int UserId)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT AVG(EXTRACT(EPOCH FROM duration)/86400) FROM PeriodCycle WHERE user_id = @UserId";
                cmd.Parameters.Add("@UserId", NpgsqlDbType.Integer).Value = UserId;
                
                dbConn.Open();
                var result = cmd.ExecuteScalar();
                
                if (result == DBNull.Value)
                {
                    return 0;
                }
                
                return Convert.ToDouble(result);
            }
            finally
            {
                dbConn?.Close();
            }
        }
    }
}
