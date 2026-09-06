namespace MakeReady.Models;

public enum AmmoType
{
    FMJ,          // Full Metal Jacket
    HollowPoint,  // JHP / HP
    FlatPoint,    // Lead Flat Point / LSWC
    SoftPoint,    // Soft Point / JSP
    FMJFlatPoint, // FMJFP
    Frangible,    // Disintegrating / training
    Subsonic,     // Suppressor / subsonic loads
    MatchHP,      // BTHP / OTM precision
    Tracer,
    Other
}

public class Ammo
{
    public int Id { get; set; }
    public string Brand { get; set; } = "";
    public string? Caliber { get; set; }
    public int Grain { get; set; }
    public AmmoType Type { get; set; }
    public string? Notes { get; set; }

    public string TypeLabel => Type switch
    {
        AmmoType.FMJ          => "FMJ",
        AmmoType.HollowPoint  => "JHP",
        AmmoType.FlatPoint    => "Flat Point",
        AmmoType.SoftPoint    => "Soft Point",
        AmmoType.FMJFlatPoint => "FMJ-FP",
        AmmoType.Frangible    => "Frangible",
        AmmoType.Subsonic     => "Subsonic",
        AmmoType.MatchHP      => "Match HP",
        AmmoType.Tracer       => "Tracer",
        _                     => "Other"
    };

    public string DisplayName => $"{Brand} {Grain}gr {TypeLabel}" + (Caliber != null ? $" ({Caliber})" : "");
    public string ShortName   => $"{Brand} {Grain}gr {TypeLabel}";
}
