using System.Text.Json.Serialization;

namespace MakeReady.Models;

// A single logged run of a Drill. No hit/score tracking -- this app can't
// detect shots without dedicated hardware, so a dry-fire session just
// records that a rep happened, how long it took, and against which gun.
public class DrillSession
{
    public int Id { get; set; }
    public int DrillId { get; set; }
    [JsonIgnore] public Drill Drill { get; set; } = null!;

    public int? FirearmId { get; set; }
    [JsonIgnore] public Firearm? Firearm { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public double ElapsedSeconds { get; set; }
    public string? Notes { get; set; }
}
