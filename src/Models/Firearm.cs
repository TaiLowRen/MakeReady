using System.Text.Json.Serialization;

namespace MakeReady.Models;

public class Firearm
{
    public int Id { get; set; }
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FirearmCategory Category { get; set; } = FirearmCategory.Handgun;

    [JsonIgnore] public List<Modification> Modifications { get; set; } = new();
    [JsonIgnore] public List<Magazine> Magazines { get; set; } = new();
    [JsonIgnore] public List<RoundSession> Sessions { get; set; } = new();
    [JsonIgnore] public List<FirearmMalfunction> Malfunctions { get; set; } = new();
    [JsonIgnore] public MaintenanceSchedule? Schedule { get; set; }
    [JsonIgnore] public List<MaintenancePart> Parts { get; set; } = new();
    [JsonIgnore] public List<MaintenanceLog> MaintenanceLogs { get; set; } = new();

    [JsonIgnore] public int TotalRoundsFired => Sessions.Sum(s => s.RoundsFired);
    [JsonIgnore] public string DisplayName => $"{Make} {Model}".Trim();
    [JsonIgnore] public double MalfunctionRate => TotalRoundsFired > 0 ? (double)Malfunctions.Count / TotalRoundsFired * 100 : 0;
    [JsonIgnore] public string CategoryLabel => Category == FirearmCategory.Handgun ? "Handgun" : Category == FirearmCategory.LongGun ? "Long Gun" : "Other";
}
