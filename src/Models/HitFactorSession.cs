using System.Text.Json.Serialization;

namespace MakeReady.Models;

public class HitFactorSession
{
    public int Id { get; set; }
    public int? FirearmId { get; set; }
    [JsonIgnore] public Firearm? Firearm { get; set; }
    public string Name { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Location { get; set; }
    public string? Notes { get; set; }

    [JsonIgnore] public List<HitFactorStage> Stages { get; set; } = new();

    [JsonIgnore] public double BestHitFactor  => Stages.Count > 0 ? Stages.Max(s => s.HitFactor)     : 0;
    [JsonIgnore] public double WorstHitFactor => Stages.Count > 0 ? Stages.Min(s => s.HitFactor)     : 0;
    [JsonIgnore] public double AverageHitFactor => Stages.Count > 0 ? Stages.Average(s => s.HitFactor) : 0;
    [JsonIgnore] public int TotalPoints => Stages.Sum(s => s.NetPoints);
}
