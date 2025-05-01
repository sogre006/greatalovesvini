using Npgsql;
using Microsoft.Extensions.Configuration;

namespace PeriodTracker.Model.Repositories;

public class BaseRepository
{
    protected string ConnectionString { get; }
    
    public BaseRepository(IConfiguration configuration)
    {
        ConnectionString = configuration.GetConnectionString("period_tracker_db");
        Console.WriteLine("[BaseRepository] Connection string initialized (masked): " + 
                        (ConnectionString?.Replace("Password=", "Password=***") ?? "NULL CONNECTION STRING!"));
    }
    
    protected NpgsqlDataReader GetData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        try
        {
            conn.Open();
            Console.WriteLine($"[BaseRepository] Executing query: {cmd.CommandText.Substring(0, Math.Min(50, cmd.CommandText.Length))}...");
            return cmd.ExecuteReader();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BaseRepository] Error executing query: {ex.Message}");
            throw; // Rethrow to allow proper handling up the stack
        }
    }
    
    protected bool InsertData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        try
        {
            conn.Open();
            Console.WriteLine($"[BaseRepository] Executing insert: {cmd.CommandText.Substring(0, Math.Min(50, cmd.CommandText.Length))}...");
            int rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"[BaseRepository] Insert affected {rowsAffected} rows");
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BaseRepository] Error executing insert: {ex.Message}");
            return false;
        }
    }
    
    protected bool UpdateData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        try
        {
            conn.Open();
            Console.WriteLine($"[BaseRepository] Executing update: {cmd.CommandText.Substring(0, Math.Min(50, cmd.CommandText.Length))}...");
            int rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"[BaseRepository] Update affected {rowsAffected} rows");
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BaseRepository] Error executing update: {ex.Message}");
            return false;
        }
    }
    
    protected bool DeleteData(NpgsqlConnection conn, NpgsqlCommand cmd)
    {
        try
        {
            conn.Open();
            Console.WriteLine($"[BaseRepository] Executing delete: {cmd.CommandText.Substring(0, Math.Min(50, cmd.CommandText.Length))}...");
            int rowsAffected = cmd.ExecuteNonQuery();
            Console.WriteLine($"[BaseRepository] Delete affected {rowsAffected} rows");
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BaseRepository] Error executing delete: {ex.Message}");
            return false;
        }
    }
}