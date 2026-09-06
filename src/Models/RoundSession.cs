namespace MakeReady.Models;

public class RoundSession
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    public Firearm Firearm { get; set; } = null!;
    public int? MagazineId { get; set; }
    public Magazine? Magazine { get; set; }
    public int RoundsFired { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public TrackingMode Mode { get; set; }

    public int? AmmoId { get; set; }
    public Ammo? Ammo { get; set; }
}

public enum TrackingMode { Magazine, Tally }
