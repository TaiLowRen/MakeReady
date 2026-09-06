namespace MakeReady.Models;

public class HitFactorStage
{
    public int Id { get; set; }
    public int HitFactorSessionId { get; set; }
    public HitFactorSession Session { get; set; } = null!;

    public string? StageName { get; set; }

    // Hits
    public int AHits { get; set; }
    public int CHits { get; set; }
    public int DHits { get; set; }

    // Penalties
    public int Misses { get; set; }
    public int NoShoots { get; set; }
    public int Procedurals { get; set; }

    public double Time { get; set; }

    // Derived
    public int ScoredPoints => (AHits * 5) + (CHits * 3) + (DHits * 1);
    public int Penalties    => (Misses * 10) + (NoShoots * 10) + (Procedurals * 10);
    public int NetPoints    => ScoredPoints - Penalties;
    public int TotalHits    => AHits + CHits + DHits + Misses;
    public double HitFactor => Time > 0 ? Math.Round(Math.Max(0d, NetPoints / Time), 4) : 0;

    public string HitFactorClass => HitFactor switch
    {
        >= 8  => "GM",
        >= 5  => "M",
        >= 3  => "A",
        >= 2  => "B",
        >= 1  => "C",
        _     => "D"
    };

    public string HitFactorColor => HitFactor switch
    {
        >= 8  => "#d4a017",
        >= 5  => "#c0392b",
        >= 3  => "#3498db",
        >= 2  => "#27ae60",
        >= 1  => "#7f8c8d",
        _     => "#8b949e"
    };
}
