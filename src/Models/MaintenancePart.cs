using System.Text.Json.Serialization;

namespace MakeReady.Models;

public class MaintenancePart
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    [JsonIgnore] public Firearm Firearm { get; set; } = null!;
    public string Name { get; set; } = "";
    public string? Brand { get; set; }
    public int? IntervalDays { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
}

public static class DefaultParts
{
    public static readonly string[] Handgun =
    [
        "Barrel / Bore",
        "Slide",
        "Frame",
        "Trigger Group",
        "Recoil Spring Assembly",
        "Magazine Feed Lips",
        "Sights / Optic"
    ];

    public static readonly string[] LongGun =
    [
        "Barrel / Bore",
        "Bolt Carrier Group",
        "Upper Receiver",
        "Lower Receiver",
        "Trigger Group",
        "Gas System",
        "Handguard / Rail",
        "Stock / Buffer"
    ];
}
