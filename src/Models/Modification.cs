namespace MakeReady.Models;

public enum ModificationType { Modification, Accessory }

public class Modification
{
    public int Id { get; set; }
    public int FirearmId { get; set; }
    public Firearm Firearm { get; set; } = null!;
    public string Name { get; set; } = "";
    public ModificationType Type { get; set; }
    public string? Notes { get; set; }
}
