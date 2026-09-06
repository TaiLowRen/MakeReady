namespace MakeReady.Models;

public class HitFactorSession
{
    public int Id { get; set; }
    public int? FirearmId { get; set; }
    public Firearm? Firearm { get; set; }
    public string Name { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Location { get; set; }
    public string? Notes { get; set; }

    public List<HitFactorStage> Stages { get; set; } = new();

    public double BestHitFactor  => Stages.Count > 0 ? Stages.Max(s => s.HitFactor)     : 0;
    public double WorstHitFactor => Stages.Count > 0 ? Stages.Min(s => s.HitFactor)     : 0;
    public double AverageHitFactor => Stages.Count > 0 ? Stages.Average(s => s.HitFactor) : 0;
    public int TotalPoints => Stages.Sum(s => s.NetPoints);
}
