
namespace PeriodTracker.Model.Entities;

public class CycleEntry
{
    public CycleEntry(int id)
    {
        EntryId = id;
    }

    public int EntryId { get; set; }

    public int CycleId { get; set; }

    public int CalendarId { get; set; }

    public DateTime Date { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    public PeriodCycle PeriodCycle { get; set; } //foreign key from PeriodCycle
    public Calendar Calendar { get; set; } //foreign key from Calendar
}
