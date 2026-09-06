namespace MakeReady.Models;

public class MaintenanceSchedule
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    public Firearm Firearm { get; set; } = null!;
    public int IntervalDays { get; set; } = 90;
    public bool NotificationsEnabled { get; set; } = true;
}
