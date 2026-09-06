namespace MakeReady.Models;

public enum MalfunctionType
{
    FailureToFeed,
    FailureToEject,
    DoubleFeed,
    Stovepipe,
    FailureToFire,
    FailureToLock,
    Other
}

public class MagazineMalfunction
{
    public int Id { get; set; }
    public int MagazineId { get; set; }
    public Magazine Magazine { get; set; } = null!;
    public MalfunctionType Type { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public int? AmmoId { get; set; }
    public Ammo? Ammo { get; set; }

    public string TypeLabel => Type switch
    {
        MalfunctionType.FailureToFeed   => "Failure to Feed",
        MalfunctionType.FailureToEject  => "Failure to Eject",
        MalfunctionType.DoubleFeed      => "Double Feed",
        MalfunctionType.Stovepipe       => "Stovepipe",
        MalfunctionType.FailureToFire   => "Failure to Fire",
        MalfunctionType.FailureToLock   => "Failure to Lock",
        _                               => "Other"
    };
}
