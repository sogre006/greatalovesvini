namespace PeriodTracker.Model.Entities;

using System.Text.Json.Serialization;

public class User
{
    public User(int id)
    {
        userId = id;
    }

    public int userId { get; set; } // Using camelCase to match frontend expectations
    public string name { get; set; }
    public string email { get; set; }
    public string pw { get; set; } // Database column name is pw
    public DateTime createdAt { get; set; } = DateTime.Now;

    // Navigation properties
    [JsonIgnore] // Prevent circular references
    public List<PeriodCycle> PeriodCycles { get; set; } = new List<PeriodCycle>();
    
    [JsonIgnore] // Prevent circular references
    public List<Calendar> Calendars { get; set; } = new List<Calendar>();
}