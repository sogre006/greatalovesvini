namespace PeriodTracker.Model.Entities;
public class PeriodCycle
{
    public PeriodCycle(int id)
    {
        CycleId = id;
    }

    public int CycleId { get; set; }
    public int UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // This is a computed column in the database
    public TimeSpan Duration => EndDate - StartDate;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public User User { get; set; } //foreign key from User
                                   //public ICollection<CycleEntry> CycleEntries { get; set; }
}
