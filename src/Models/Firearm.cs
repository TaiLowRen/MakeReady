namespace MakeReady.Models;

public class Firearm
{
    public int Id { get; set; }
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FirearmCategory Category { get; set; } = FirearmCategory.Handgun;

    public List<Modification> Modifications { get; set; } = new();
    public List<Magazine> Magazines { get; set; } = new();
    public List<RoundSession> Sessions { get; set; } = new();
    public List<FirearmMalfunction> Malfunctions { get; set; } = new();
    public MaintenanceSchedule? Schedule { get; set; }
    public List<MaintenancePart> Parts { get; set; } = new();
    public List<MaintenanceLog> MaintenanceLogs { get; set; } = new();

    public int TotalRoundsFired => Sessions.Sum(s => s.RoundsFired);
    public string DisplayName => $"{Make} {Model}".Trim();
    public double MalfunctionRate => TotalRoundsFired > 0 ? (double)Malfunctions.Count / TotalRoundsFired * 100 : 0;
    public string CategoryLabel => Category == FirearmCategory.Handgun ? "Handgun" : Category == FirearmCategory.LongGun ? "Long Gun" : "Other";
}
