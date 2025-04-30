using Microsoft.Extensions.Configuration;
using PeriodTracker.Model.Entities;
using Npgsql;
using NpgsqlTypes;

namespace PeriodTracker.Model.Repositories
{
    public class UserRepository : BaseRepository
    {
        public UserRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public User GetUserById(int id)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Users WHERE user_id = @id";
                cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
                
                var data = GetData(dbConn, cmd);
                if (data != null && data.Read())
                {
                    return new User(Convert.ToInt32(data["user_id"]))
                    {
                        name = data["name"].ToString(),
                        email = data["email"].ToString(),
                        pw = data["pw"].ToString(),
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

        public User GetUserByEmail(string email)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Users WHERE email = @email";
                cmd.Parameters.Add("@email", NpgsqlDbType.Text).Value = email;
                
                var data = GetData(dbConn, cmd);
                if (data != null && data.Read())
                {
                    return new User(Convert.ToInt32(data["user_id"]))
                    {
                        name = data["name"].ToString(),
                        email = data["email"].ToString(),
                        pw = data["pw"].ToString(),
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

        public List<User> GetUsers()
        {
            NpgsqlConnection dbConn = null;
            var users = new List<User>();
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Users";
                
                var data = GetData(dbConn, cmd);
                if (data != null)
                {
                    while (data.Read())
                    {
                        User user = new User(Convert.ToInt32(data["user_id"]))
                        {
                            name = data["name"].ToString(),
                            email = data["email"].ToString(),
                            pw = data["pw"].ToString(),
                            createdAt = Convert.ToDateTime(data["created_at"])
                        };
                        users.Add(user);
                    }
                }
                return users;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool InsertUser(User user)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Users 
                    (name, email, pw, created_at)
                    VALUES 
                    (@name, @email, @pw, @createdAt)
                    RETURNING user_id";
                
                cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, user.name);
                cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, user.email);
                cmd.Parameters.AddWithValue("@pw", NpgsqlDbType.Text, user.pw);
                cmd.Parameters.AddWithValue("@createdAt", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
                
                dbConn.Open();
                // Get the newly created user ID
                var userId = Convert.ToInt32(cmd.ExecuteScalar());
                user.userId = userId;
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting user: {ex.Message}");
                return false;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool UpdateUser(User user)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE Users SET
                    name = @name,
                    email = @email
                    WHERE user_id = @userId";
                
                cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, user.name);
                cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, user.email);
                cmd.Parameters.AddWithValue("@userId", NpgsqlDbType.Integer, user.userId);
                
                bool result = UpdateData(dbConn, cmd);
                return result;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool UpdateUserPassword(int userId, string password)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = @"
                    UPDATE Users SET
                    pw = @pw
                    WHERE user_id = @userId";
                
                cmd.Parameters.AddWithValue("@pw", NpgsqlDbType.Text, password);
                cmd.Parameters.AddWithValue("@userId", NpgsqlDbType.Integer, userId);
                
                bool result = UpdateData(dbConn, cmd);
                return result;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool DeleteUser(int id)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "DELETE FROM Users WHERE user_id = @id";
                cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);
                
                bool result = DeleteData(dbConn, cmd);
                return result;
            }
            finally
            {
                dbConn?.Close();
            }
        }

        public bool EmailExists(string email)
        {
            NpgsqlConnection dbConn = null;
            try
            {
                dbConn = new NpgsqlConnection(ConnectionString);
                var cmd = dbConn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE email = @email";
                cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, email);
                
                dbConn.Open();
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
            finally
            {
                dbConn?.Close();
            }
        }
    }
}