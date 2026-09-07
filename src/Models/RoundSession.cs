using System.Text.Json.Serialization;

namespace MakeReady.Models;

public class RoundSession
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    [JsonIgnore] public Firearm Firearm { get; set; } = null!;
    public int? MagazineId { get; set; }
    [JsonIgnore] public Magazine? Magazine { get; set; }
    public int RoundsFired { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public TrackingMode Mode { get; set; }

    public int? AmmoId { get; set; }
    [JsonIgnore] public Ammo? Ammo { get; set; }
}

public enum TrackingMode { Magazine, Tally }
