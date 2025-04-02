namespace PeriodTracker.Model.Entities;

public class User
{
    public User(int id)
    {
        UserId = id;
    }

    public int UserId { get; set; } //primary key 
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; } //store as hashed in production!
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    //public ICollection<PeriodCycle> PeriodCycles { get; set; }
    //public ICollection<Calendar> Calendars { get; set; }
}



