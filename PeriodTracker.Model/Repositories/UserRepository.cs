namespace PeriodTracker.Model.Repositories;

using System;
using PeriodTracker.Model.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using global::PeriodTracker.Model.Entities;

public class UserRepository : BaseRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration) { }

    public User GetUserById(int id)
    {
        NpgsqlConnection dbConn = null;
        try
        {

            //create a new connection for database 
            dbConn = new NpgsqlConnection(ConnectionString);

            //creating an SQL command 
            var cmd = dbConn.CreateCommand();

            cmd.CommandText = "select * from user where id = @id";

            cmd.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

            //call the base method to get data 
            var data = GetData(dbConn, cmd);

            if (data != null)
            {
                if (data.Read()) //every time loop runs it reads next line from fetched rows 
                {
                    return new User(Convert.ToInt32(data["id"]))
                    {
                        UserId = Convert.ToInt32(data["id"]),
                        Name = data["name"].ToString(),
                        Email = data["email"].ToString(),
                        Password = data["password"].ToString(),
                        CreatedAt = Convert.ToDateTime(data["created_at"])
                    };

                }
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
            //create a new connection for database 
            dbConn = new NpgsqlConnection(ConnectionString);

            //creating an SQL command 
            var cmd = dbConn.CreateCommand();

            cmd.CommandText = "select * from user where email = @email";

            cmd.Parameters.Add("@email", NpgsqlDbType.Text).Value = email;

            //call the base method to get data 
            var data = GetData(dbConn, cmd);

            if (data != null)
            {
                if (data.Read()) //every time loop runs it reads next line from fetched rows 
                {
                    return new User(Convert.ToInt32(data["id"]))
                    {
                        UserId = Convert.ToInt32(data["user_id"]),
                        Name = data["name"].ToString(),
                        Email = data["email"].ToString(),
                        Password = data["password"].ToString(),
                        CreatedAt = Convert.ToDateTime(data["created_at"])
                    };

                }
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
            //create a new connection for database 
            dbConn = new NpgsqlConnection(ConnectionString);

            //creating an SQL command 
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = "select * from user";

            //call the base method to get data 
            var data = GetData(dbConn, cmd);

            if (data != null)
            {
                while (data.Read()) //every time loop runs it reads next line from fetched rows 
                {
                    User u = new User(Convert.ToInt32(data["id"]))
                    {
                        UserId = Convert.ToInt32(data["user_id"]),
                        Name = data["name"].ToString(),
                        Email = data["email"].ToString(),
                        Password = data["password"].ToString(),
                        CreatedAt = Convert.ToDateTime(data["created_at"])
                    };

                    users.Add(u);
                }
            }
            return users;
        }
        finally
        {
            dbConn?.Close();
        }

    }

    //add a new user 
    public bool InsertUser(User u)
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();
            cmd.CommandText = @" 
insert into users 
(user_id, name, email, password, created_at) 
values
(@user_id, @name, @email, @password, @created_at)
";

            //adding parameters in a better way 
            cmd.Parameters.AddWithValue("@user_id", NpgsqlDbType.Integer, u.UserId);
            cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, u.Name);
            cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, u.Email);
            cmd.Parameters.AddWithValue("@password", NpgsqlDbType.Text, u.Password);
            cmd.Parameters.AddWithValue("@created_at", NpgsqlDbType.Timestamp, u.CreatedAt);

            //will return true if all goes well 
            bool result = InsertData(dbConn, cmd);

            return result;
        }
        finally
        {
            dbConn?.Close();
        }
    }

    public bool UpdateUser(User u)
    {
        using var dbConn = new NpgsqlConnection(ConnectionString);
        var cmd = dbConn.CreateCommand();
        cmd.CommandText = @" 

     update users set
            name = @name,
            email = @email,
            password = @password,
            created_at = @created_at
      where 
      user_id = @user_id";

        cmd.Parameters.AddWithValue("@user_id", NpgsqlDbType.Integer, u.UserId);
        cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, u.Name);
        cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, u.Email);
        cmd.Parameters.AddWithValue("@password", NpgsqlDbType.Text, u.Password);
        cmd.Parameters.AddWithValue("@created_at", NpgsqlDbType.Timestamp, u.CreatedAt);

        bool result = UpdateData(dbConn, cmd);
        return result;
    }
    public bool UpdateUserPassword(int user_Id, string password)

    {
        using var dbConn = new NpgsqlConnection(ConnectionString);
        var cmd = dbConn.CreateCommand();
        cmd.CommandText = @" 

     update users set
            name = @name,
            password = @password,
      where 
      user_id = @user_id";

        cmd.Parameters.AddWithValue("@password", NpgsqlDbType.Text, password);
        cmd.Parameters.AddWithValue("@user_id", NpgsqlDbType.Integer, u.UserId);


        bool result = UpdateData(dbConn, cmd);
        return result;
    }

    public bool DeleteUser(int id)
    {
        var dbConn = new NpgsqlConnection(ConnectionString);
        var cmd = dbConn.CreateCommand();
        cmd.CommandText = @" 
delete from user 
where id = @id ";

        //adding parameters in a better way 
        cmd.Parameters.AddWithValue("@id", NpgsqlDbType.Integer, id);

        //will return true if all goes well 
        bool result = DeleteData(dbConn, cmd);

        return result;
    }

    public bool EmailExists(string email)
    {
        NpgsqlConnection dbConn = null;
        try
        {
            dbConn = new NpgsqlConnection(ConnectionString);
            var cmd = dbConn.CreateCommand();

            // check if email exists
            cmd.CommandText = "select count(*) from users where email = @email";

            // add parameter
            cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, email);

            // open connection and execute
            dbConn.Open();
            var count = Convert.ToInt32(cmd.ExecuteScalar());

            // return true if count > 0
            return count > 0;
        }
        finally
        {
            dbConn?.Close();
        }
    }
}


---

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
                        password = data["password"].ToString(),
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
                        password = data["password"].ToString(),
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
                            password = data["password"].ToString(),
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
                    (name, email, password, created_at)
                    VALUES 
                    (@name, @email, @password, @createdAt)
                    RETURNING user_id";
                
                cmd.Parameters.AddWithValue("@name", NpgsqlDbType.Text, user.name);
                cmd.Parameters.AddWithValue("@email", NpgsqlDbType.Text, user.email);
                cmd.Parameters.AddWithValue("@password", NpgsqlDbType.Text, user.password);
                cmd.Parameters.AddWithValue("@createdAt", NpgsqlDbType.TimestampTz, DateTime.UtcNow);
                
                dbConn.Open();
                // Get the newly created user ID
                var userId = Convert.ToInt32(cmd.ExecuteScalar());
                user.userId = userId;
                
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
                    password = @password
                    WHERE user_id = @userId";
                
                cmd.Parameters.AddWithValue("@password", NpgsqlDbType.Text, password);
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
