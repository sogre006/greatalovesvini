using System.ComponentModel.DataAnnotations; //for required range

namespace PeriodTracker.Model.Entities;

public class Calendar
{

    public Calendar(int id)
    {
        CalendarId = id;
    }

    public int CalendarId { get; set; }

    public int UserId { get; set; }

//you can only put 1 to 12 for month:
    [Required]
    [Range(1, 12)]
    public short Month { get; set; }

    public short Year { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public User User { get; set; } //foreign key from User
    //public ICollection<CycleEntry> CycleEntries { get; set; }
}