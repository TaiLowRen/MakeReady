using System.Text.Json.Serialization;

namespace MakeReady.Models;

public class MaintenanceSchedule
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    [JsonIgnore] public Firearm Firearm { get; set; } = null!;
    public int IntervalDays { get; set; } = 90;
    public bool NotificationsEnabled { get; set; } = true;
}
