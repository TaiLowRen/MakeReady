namespace MakeReady.Models;

public class MagazineModification
{
    public int Id { get; set; }
    public int MagazineId { get; set; }
    public Magazine Magazine { get; set; } = null!;
    public string Name { get; set; } = "";
    public ModificationType Type { get; set; }
    public string? Notes { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
