using System.Text.Json.Serialization;

namespace MakeReady.Models;

public class Drill
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public double RandomDelayMinSeconds { get; set; } = 1.0;
    public double RandomDelayMaxSeconds { get; set; } = 3.0;
    public double? ParTimeSeconds { get; set; }

    [JsonIgnore] public List<DrillTarget> Targets { get; set; } = new();
    [JsonIgnore] public List<DrillSession> Sessions { get; set; } = new();

    [JsonIgnore]
    public double TotalDurationSeconds =>
        Targets.Count == 0 ? 0 : Targets.Max(t => t.AppearAtSeconds + t.DurationSeconds);
}
